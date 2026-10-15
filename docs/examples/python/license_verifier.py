# Copyright (c) 2026 Gonzalo Martínez García.
# License Generator integration example: you may use, copy, modify and distribute this file
# under section 2 of the License Generator EULA, keeping this notice.
"""Reference License Generator verifier for Python 3.11 or later.

It needs only the `cryptography` package and does no I/O: the caller supplies
the trusted public key, the expected product and hardware ID, and the current
time. Its results match the C# reference verifier for every published test
vector.
"""
from __future__ import annotations

import base64
import binascii
from dataclasses import dataclass
from datetime import date, datetime, timedelta, timezone
from enum import StrEnum
import hashlib
import json
import re
import unicodedata

from cryptography.exceptions import InvalidSignature, UnsupportedAlgorithm
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import padding, rsa

MAXIMUM_ENCODED_LENGTH = 131072
MAXIMUM_JSON_BYTES = MAXIMUM_ENCODED_LENGTH // 4 * 3
MAXIMUM_JSON_DEPTH = 8
MAXIMUM_PUBLIC_KEY_LENGTH = 65536
MINIMUM_KEY_SIZE = 2048
MAX_APP_ID_LENGTH = 128
MAX_APP_ID_UTF8_BYTES = 255
MAX_REGISTRATION_NAME_LENGTH = 1024
MAX_HARDWARE_ID_LENGTH = 1024
MAX_USER_DEFINED_LENGTH = 8192


class InspectionFailure(StrEnum):
    MALFORMED_LICENSE = "malformed"
    UNSUPPORTED_FORMAT = "unsupported-format"
    INVALID_EXPECTED_PRODUCT = "invalid-expected-product"
    INVALID_PUBLIC_KEY = "invalid-public-key"
    SIGNATURE_MISMATCH = "signature-mismatch"


class ProductCheck(StrEnum):
    MATCH = "match"
    MISMATCH = "mismatch"


class HardwareCheck(StrEnum):
    NOT_BOUND = "not-bound"
    NOT_CHECKED = "not-checked"
    MATCH = "match"
    MISMATCH = "mismatch"


class ExpirationCheck(StrEnum):
    NEVER_EXPIRES = "never"
    CURRENT = "current"
    EXPIRED = "expired"


@dataclass(frozen=True)
class VerifiedLicense:
    """The signed claims of a license. revocation_id is the lowercase hexadecimal SHA-256 of the decoded
    signature: every encoding of one license has the same identifier, so compare it with a revocation list."""

    app_id: str
    registration_name: str
    hardware_id: str
    expiration_date: date | None
    user_defined: str | None
    format_version: int | None
    revocation_id: str


@dataclass(frozen=True)
class Rejected:
    reason: InspectionFailure


@dataclass(frozen=True)
class Authenticated:
    """Authenticated claims may still fail the caller's product, hardware or expiration policy."""

    license: VerifiedLicense
    product: ProductCheck
    hardware: HardwareCheck
    expiration: ExpirationCheck
    checked_at_utc: datetime

    @property
    def is_valid(self) -> bool:
        return (self.product is ProductCheck.MATCH
                and self.hardware in (HardwareCheck.NOT_BOUND, HardwareCheck.MATCH)
                and self.expiration in (ExpirationCheck.NEVER_EXPIRES, ExpirationCheck.CURRENT))


def verify(encoded_license: str, public_key_pem: str, expected_app_id: str, expected_hardware_id: str,
           now_utc: datetime) -> VerifiedLicense | None:
    """Returns the license only when it is authentic and valid for this product, machine and time."""
    if expected_hardware_id is None:
        return None
    result = inspect(encoded_license, public_key_pem, expected_app_id, expected_hardware_id, now_utc)
    return result.license if isinstance(result, Authenticated) and result.is_valid else None


def inspect(encoded_license: str, public_key_pem: str, expected_app_id: str, expected_hardware_id: str | None,
            now_utc: datetime) -> Rejected | Authenticated:
    """Authenticates first, then reports policy checks. A None expected hardware ID explicitly skips that check."""
    checked_at = _utc(now_utc)
    if not isinstance(expected_app_id, str) or _is_blank(expected_app_id):
        return Rejected(InspectionFailure.INVALID_EXPECTED_PRODUCT)
    if (not isinstance(public_key_pem, str) or _is_blank(public_key_pem)
            or _utf16_length(public_key_pem) > MAXIMUM_PUBLIC_KEY_LENGTH):
        return Rejected(InspectionFailure.INVALID_PUBLIC_KEY)
    try:
        claims, signed_bytes, signature = _read_envelope(encoded_license)
    except _EnvelopeError as error:
        return Rejected(error.failure)
    try:
        key = _read_public_key(public_key_pem)
    except (ValueError, UnsupportedAlgorithm):
        return Rejected(InspectionFailure.INVALID_PUBLIC_KEY)
    try:
        key.verify(signature, signed_bytes, padding.PKCS1v15(), hashes.SHA256())
    except InvalidSignature:
        return Rejected(InspectionFailure.SIGNATURE_MISMATCH)

    product = ProductCheck.MATCH if claims.app_id == expected_app_id else ProductCheck.MISMATCH
    if not claims.hardware_id:
        hardware = HardwareCheck.NOT_BOUND
    elif expected_hardware_id is None:
        hardware = HardwareCheck.NOT_CHECKED
    else:
        hardware = HardwareCheck.MATCH if claims.hardware_id == expected_hardware_id else HardwareCheck.MISMATCH
    if claims.expiration_date is None:
        expiration = ExpirationCheck.NEVER_EXPIRES
    else:
        expiry = claims.expiration_date
        expires_at = datetime(expiry.year, expiry.month, expiry.day, tzinfo=timezone.utc)
        expiration = ExpirationCheck.EXPIRED if checked_at >= expires_at else ExpirationCheck.CURRENT
    return Authenticated(claims, product, hardware, expiration, checked_at)


def check_format(encoded_license: str) -> InspectionFailure | None:
    """Checks envelope syntax and limits before key I/O. Success is not authentication and exposes no claims."""
    try:
        _read_envelope(encoded_license)
    except _EnvelopeError as error:
        return error.failure
    return None


class _EnvelopeError(Exception):
    def __init__(self, failure: InspectionFailure):
        super().__init__(failure.value)
        self.failure = failure


def _malformed() -> _EnvelopeError:
    return _EnvelopeError(InspectionFailure.MALFORMED_LICENSE)


class _JsonObject(list):
    """The members of a JSON object in document order, so duplicate names stay visible."""


# Only these characters may separate Base64 groups, as in .NET's Convert.FromBase64String.
_BASE64_WHITESPACE = str.maketrans("", "", " \t\r\n")
# The ISO 8601 profile that System.Text.Json reads as a date and time; the
# published test vectors list the texts it accepts and rejects.
_ISO_DATE_TIME = re.compile(r"([0-9]{4})-([0-9]{2})-([0-9]{2})"
                            r"(?:T([0-9]{2}):([0-9]{2})(?::([0-9]{2})(?:\.[0-9]{1,16})?)?"
                            r"(?:Z|([+-])([0-9]{2})(?::([0-9]{2}))?)?)?")
_MAXIMUM_OFFSET_MINUTES = 14 * 60
_PUBLIC_KEY_PEM = re.compile(r"-----BEGIN (PUBLIC KEY|RSA PUBLIC KEY)-----([A-Za-z0-9+/=\t\n\r ]*)-----END \1-----")


def _read_envelope(encoded_license):
    if not isinstance(encoded_license, str) or _is_blank(encoded_license) or len(encoded_license) > MAXIMUM_ENCODED_LENGTH:
        raise _malformed()
    root = _parse_json(_decode_base64(encoded_license))

    app_id = _read_nonblank_text(root, "AppId")
    registration_name = _read_nonblank_text(root, "RegistrationName")
    hardware_id = _read_text(root, "HardwareId")
    signature = _read_nonblank_text(root, "Signature")
    user_defined = root.get("UserDefined")
    if user_defined is not None:
        if not isinstance(user_defined, str) or not _is_utf16_text(user_defined):
            raise _malformed()
        user_defined = user_defined or None

    format_version = root.get("FormatVersion")
    if format_version is not None:
        if type(format_version) is not int or not -2**31 <= format_version < 2**31:
            raise _malformed()
        if format_version not in (1, 2):
            raise _EnvelopeError(InspectionFailure.UNSUPPORTED_FORMAT)
    expiration_date = _read_expiration_date(root["ExpirationDate"]) if "ExpirationDate" in root else None

    signature_bytes = _decode_base64(signature)
    if not signature_bytes:
        raise _malformed()
    claims = VerifiedLicense(app_id, registration_name, hardware_id, expiration_date, user_defined, format_version,
                             hashlib.sha256(signature_bytes).hexdigest())
    signed_bytes = _data_to_sign(claims).encode("utf-8")
    return claims, signed_bytes, signature_bytes


def _decode_base64(text):
    try:
        return base64.b64decode(text.translate(_BASE64_WHITESPACE), validate=True)
    except (binascii.Error, ValueError):
        raise _malformed() from None


def _parse_json(data):
    if len(data) > MAXIMUM_JSON_BYTES:
        raise _malformed()
    try:
        root = json.loads(data.decode("utf-8"), object_pairs_hook=_JsonObject, parse_constant=_reject_constant)
    except (UnicodeDecodeError, ValueError, RecursionError):
        raise _malformed() from None
    if not isinstance(root, _JsonObject) or not _within_depth(root):
        raise _malformed()
    names = [name for name, _ in root]
    if len(set(names)) != len(names) or not all(_is_utf16_text(name) for name in names):
        raise _malformed()
    return dict(root)


def _reject_constant(name):
    raise ValueError("JSON does not allow %s." % name)


def _within_depth(root):
    pending = [(root, 1)]
    while pending:
        container, depth = pending.pop()
        if depth > MAXIMUM_JSON_DEPTH:
            return False
        children = [value for _, value in container] if isinstance(container, _JsonObject) else container
        pending.extend((child, depth + 1) for child in children if isinstance(child, list))
    return True


def _read_text(root, name):
    value = root.get(name)
    if not isinstance(value, str) or not _is_utf16_text(value):
        raise _malformed()
    return value


def _read_nonblank_text(root, name):
    value = _read_text(root, name)
    if _is_blank(value):
        raise _malformed()
    return value


def _read_expiration_date(value):
    if value is None:
        return None
    match = _ISO_DATE_TIME.fullmatch(value) if isinstance(value, str) else None
    if match is None:
        raise _malformed()
    year, month, day, hour, minute, second, sign, offset_hours, offset_minutes = match.groups()
    offset = int(offset_hours or 0) * 60 + int(offset_minutes or 0)
    if int(offset_minutes or 0) > 59 or offset > _MAXIMUM_OFFSET_MINUTES:
        raise _malformed()
    try:
        written = datetime(int(year), int(month), int(day), int(hour or 0), int(minute or 0), int(second or 0))
        # The written instant must exist in UTC, even though only its calendar date is signed.
        written - timedelta(minutes=-offset if sign == "-" else offset)
    except (ValueError, OverflowError):
        raise _malformed() from None
    # Only the calendar date is signed; a timezone or time of day cannot extend it.
    return written.date()


def _data_to_sign(license):
    app_id, name, hardware_id, user_defined = (license.app_id, license.registration_name,
                                               license.hardware_id, license.user_defined)
    if (_utf16_length(app_id) > MAX_APP_ID_LENGTH or len(app_id.encode("utf-8")) > MAX_APP_ID_UTF8_BYTES
            or _utf16_length(name) > MAX_REGISTRATION_NAME_LENGTH
            or _utf16_length(hardware_id) > MAX_HARDWARE_ID_LENGTH
            or (user_defined is not None and _utf16_length(user_defined) > MAX_USER_DEFINED_LENGTH)):
        raise _malformed()
    expiration = license.expiration_date.isoformat() if license.expiration_date else "NEVER"
    if license.format_version in (None, 1):
        if user_defined is not None or any("|" in field for field in (app_id, name, hardware_id)):
            raise _malformed()
        return "|".join((app_id, name, hardware_id, expiration))
    if user_defined is None:
        raise _malformed()
    return "LG2|" + "|".join(base64.b64encode(field.encode("utf-8")).decode("ascii")
                             for field in (app_id, name, hardware_id, expiration, user_defined))


def _read_public_key(pem):
    match = _PUBLIC_KEY_PEM.fullmatch(_strip_whitespace(pem))
    if match is None:
        raise ValueError("A public key must contain exactly one public PEM block.")
    label, body = match.groups()
    try:
        der = base64.b64decode(body.translate(_BASE64_WHITESPACE), validate=True)
    except binascii.Error as error:
        raise ValueError("The public key is not valid Base64.") from error
    body = base64.b64encode(der).decode("ascii")
    lines = "\n".join(body[index:index + 64] for index in range(0, len(body), 64))
    canonical = "-----BEGIN %s-----\n%s\n-----END %s-----\n" % (label, lines, label)
    key = serialization.load_pem_public_key(canonical.encode("ascii"))
    if not isinstance(key, rsa.RSAPublicKey) or key.key_size < MINIMUM_KEY_SIZE:
        raise ValueError("The RSA public key is invalid or smaller than 2048 bits.")
    # cryptography also reads a SubjectPublicKeyInfo body under this label; the contract does not.
    if label == "RSA PUBLIC KEY" and key.public_bytes(serialization.Encoding.DER, serialization.PublicFormat.PKCS1) != der:
        raise ValueError("An RSA PUBLIC KEY block must hold a PKCS#1 key.")
    return key


def _utc(now_utc):
    if not isinstance(now_utc, datetime) or now_utc.utcoffset() is None:
        raise ValueError("now_utc must be a timezone-aware datetime.")
    return now_utc.astimezone(timezone.utc)


def _is_utf16_text(text):
    # A lone surrogate escape is valid JSON but not text that .NET can read or sign.
    return not any(0xD800 <= ord(character) <= 0xDFFF for character in text)


def _utf16_length(text):
    return len(text) + sum(1 for character in text if ord(character) > 0xFFFF)


def _is_whitespace(character):
    return character in "\t\n\v\f\r\x85" or unicodedata.category(character) in ("Zs", "Zl", "Zp")


def _is_blank(text):
    return all(_is_whitespace(character) for character in text)


def _strip_whitespace(text):
    start, end = 0, len(text)
    while start < end and _is_whitespace(text[start]):
        start += 1
    while end > start and _is_whitespace(text[end - 1]):
        end -= 1
    return text[start:end]
