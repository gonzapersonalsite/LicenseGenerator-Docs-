# Copyright (c) 2026 Gonzalo Martínez García.
# License Generator integration example: you may use, copy, modify and distribute this file
# under section 2 of the License Generator EULA, keeping this notice.
"""Checks the Python reference verifier against the published test vectors.

    python -m unittest test_license_verifier    (from this directory)
"""
import base64
from datetime import datetime, timedelta, timezone
import hashlib
import hmac
import json
from pathlib import Path
import unittest

from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.asymmetric import rsa

import license_verifier
import license_verifier as verifier

VECTORS = Path(__file__).resolve().parent.parent / "test-vectors.json"
PUBLISHED_HARDWARE_ID = object()
MALFORMED = {"result": "rejected", "reason": "malformed"}


def describe(result):
    """The published, language-neutral form of an inspection result."""
    if isinstance(result, verifier.Rejected):
        return {"result": "rejected", "reason": result.reason.value}
    claims = result.license
    return {"result": "authenticated", "valid": result.is_valid, "product": result.product.value,
            "hardware": result.hardware.value, "expiration": result.expiration.value,
            "formatVersion": claims.format_version or 1, "appId": claims.app_id,
            "registrationName": claims.registration_name,
            "hardwareId": claims.hardware_id,
            "expirationDate": claims.expiration_date.isoformat() if claims.expiration_date else None,
            "userDefined": claims.user_defined, "revocationId": claims.revocation_id}


class VectorTestCase(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.vectors = json.loads(VECTORS.read_text(encoding="utf-8"))
        cls.cases = {case["name"]: case for case in cls.vectors["cases"]}
        cls.now = datetime.fromisoformat(cls.vectors["checkedAtUtc"])

    def inspect(self, license, public_key=None, hardware_id=PUBLISHED_HARDWARE_ID, now=None):
        return verifier.inspect(license, public_key or self.vectors["publicKey"], self.vectors["expectedAppId"],
                                self.vectors["expectedHardwareId"] if hardware_id is PUBLISHED_HARDWARE_ID else hardware_id,
                                now or self.now)

    def case_inputs(self, case):
        """A case's inputs: its own key, product or hardware ID where it sets one, a null hardware ID included."""
        return (case["license"], case.get("publicKey", self.vectors["publicKey"]),
                case.get("expectedAppId", self.vectors["expectedAppId"]),
                case.get("expectedHardwareId", self.vectors["expectedHardwareId"]), self.now)

    def edited(self, name, replace, with_text):
        """A vector's license with its JSON text edited, as an attacker or a broken client could send it."""
        text = base64.b64decode(self.cases[name]["license"]).decode("utf-8")
        self.assertEqual(1, text.count(replace))
        return base64.b64encode(text.replace(replace, with_text).encode("utf-8")).decode("ascii")


class PublishedVectorTests(VectorTestCase):
    def test_every_vector_gives_its_published_result(self):
        self.assertGreater(len(self.cases), 0)
        for name, case in self.cases.items():
            with self.subTest(vector=name):
                self.assertEqual(case["expected"], describe(verifier.inspect(*self.case_inputs(case))))

    def test_every_published_expiration_text_is_read_as_declared(self):
        texts = self.vectors["expirationTexts"]
        source = self.cases[texts["source"]]
        boundary = {at: datetime.fromisoformat(texts["boundary"][at]) for at in ("currentAt", "expiredAt")}
        self.assertTrue(texts["accepted"] and texts["rejected"])
        for text, expected in [(text, source["expected"]) for text in texts["accepted"]] + \
                [(text, MALFORMED) for text in texts["rejected"]]:
            with self.subTest(text=text):
                license = self.edited(texts["source"], json.dumps(texts["written"]), json.dumps(text))
                self.assertEqual(expected, describe(self.inspect(license)))
                if expected is not MALFORMED:
                    self.assertEqual(verifier.ExpirationCheck.CURRENT, self.inspect(license, now=boundary["currentAt"]).expiration)
                    self.assertEqual(verifier.ExpirationCheck.EXPIRED, self.inspect(license, now=boundary["expiredAt"]).expiration)

    def test_verify_returns_the_license_only_when_it_is_valid_and_the_hardware_is_expected(self):
        for name, case in self.cases.items():
            with self.subTest(vector=name):
                inputs = self.case_inputs(case)
                license = verifier.verify(*inputs)
                if case["expected"].get("valid") and inputs[3] is not None:
                    self.assertEqual(case["expected"]["registrationName"], license.registration_name)
                else:
                    self.assertIsNone(license)

    def test_format_check_rejects_without_a_key(self):
        self.assertIsNone(verifier.check_format(self.cases["format2-user-defined"]["license"]))
        self.assertEqual(verifier.InspectionFailure.MALFORMED_LICENSE, verifier.check_format("This is not a license."))
        self.assertEqual(verifier.InspectionFailure.UNSUPPORTED_FORMAT,
                         verifier.check_format(self.cases["unsupported-format-version"]["license"]))


class CallerInputTests(VectorTestCase):
    """The rules the shared vectors cannot carry: private keys, whitespace around keys and Python's clock."""

    def test_public_keys_the_vectors_cannot_carry_are_rejected(self):
        private = rsa.generate_private_key(public_exponent=65537, key_size=2048)
        rejected = {
            "blank": " ",
            "private key": private.private_bytes(serialization.Encoding.PEM, serialization.PrivateFormat.PKCS8,
                                                 serialization.NoEncryption()).decode("ascii"),
            "two blocks": self.vectors["publicKey"] + "\n" + self.vectors["publicKey"],
            "oversized": "-----BEGIN PUBLIC KEY-----\n" + "A" * verifier.MAXIMUM_PUBLIC_KEY_LENGTH,
        }
        license = self.cases["legacy-bound-perpetual"]["license"]
        for name, pem in rejected.items():
            with self.subTest(key=name):
                self.assertEqual(verifier.Rejected(verifier.InspectionFailure.INVALID_PUBLIC_KEY),
                                 self.inspect(license, public_key=pem))
        accepted = self.inspect(license, public_key="\n  " + self.vectors["publicKey"] + "  \n")
        self.assertEqual(self.cases["legacy-bound-perpetual"]["expected"], describe(accepted))

    def test_public_key_length_counts_utf16_code_units_before_the_license_is_read(self):
        one_unit_too_long = "\U0001F600" + " " * (verifier.MAXIMUM_PUBLIC_KEY_LENGTH - 1)
        self.assertEqual(verifier.Rejected(verifier.InspectionFailure.INVALID_PUBLIC_KEY),
                         self.inspect("This is not a license.", public_key=one_unit_too_long))

    def test_clock_must_carry_a_timezone(self):
        with self.assertRaises(ValueError):
            self.inspect(self.cases["legacy-bound-perpetual"]["license"], now=datetime(2027, 3, 15, 12))

    def test_expiration_starts_at_utc_midnight_of_the_signed_date(self):
        license = self.cases["legacy-expires-next-day"]["license"]
        last_moment = datetime(2027, 3, 15, 23, 59, 59, tzinfo=timezone.utc)
        self.assertEqual(verifier.ExpirationCheck.CURRENT, self.inspect(license, now=last_moment).expiration)
        same_instant_elsewhere = last_moment.astimezone(timezone(timedelta(hours=-5)))
        self.assertEqual(verifier.ExpirationCheck.CURRENT, self.inspect(license, now=same_instant_elsewhere).expiration)
        later_in_new_york = datetime(2027, 3, 15, 19, 0, 0, tzinfo=timezone(timedelta(hours=-5)))
        self.assertEqual(verifier.ExpirationCheck.EXPIRED, self.inspect(license, now=later_in_new_york).expiration)
        self.assertEqual(datetime(2027, 3, 16, tzinfo=timezone.utc), self.inspect(license, now=later_in_new_york).checked_at_utc)


class LicenseTextTests(VectorTestCase):
    def assert_malformed(self, license):
        self.assertEqual(MALFORMED, describe(self.inspect(license)))

    def test_encoded_license_size_is_limited(self):
        self.assert_malformed("A" * (verifier.MAXIMUM_ENCODED_LENGTH + 4))
        self.assert_malformed(" \r\n")

    def test_line_breaks_inside_the_base64_text_are_accepted(self):
        license = self.cases["legacy-bound-perpetual"]["license"]
        wrapped = "\r\n".join(license[index:index + 76] for index in range(0, len(license), 76))
        self.assertEqual(self.cases["legacy-bound-perpetual"]["expected"], describe(self.inspect(wrapped)))


# The integration guide quotes the marked regions below. They use what an
# application supplies, here taken from the published test vectors.
GUIDE_VECTORS = json.loads(VECTORS.read_text(encoding="utf-8"))
PUBLIC_KEY = GUIDE_VECTORS["publicKey"]
PRODUCT_ID = GUIDE_VECTORS["expectedAppId"]
STORED_CODES = []


def hardware_id():
    return GUIDE_VECTORS["expectedHardwareId"]


def save_license_code(license_code):
    STORED_CODES.append(license_code)


def load_license_code():
    return STORED_CODES[-1] if STORED_CODES else None


# guide excerpt: python-activation
def activate(license_code):
    result = license_verifier.inspect(license_code, PUBLIC_KEY, PRODUCT_ID, hardware_id(),
                                      datetime.now(timezone.utc))
    if isinstance(result, license_verifier.Authenticated) and result.is_valid:
        save_license_code(license_code)
        return "Activated for %s." % result.license.registration_name
    return activation_problem(result)


REJECTION_MESSAGES = {
    license_verifier.InspectionFailure.MALFORMED_LICENSE: "Paste the complete license code you received.",
    license_verifier.InspectionFailure.UNSUPPORTED_FORMAT: "This license needs a newer version of the application.",
    license_verifier.InspectionFailure.SIGNATURE_MISMATCH: "This license was not issued for this application.",
}


def activation_problem(result):
    if isinstance(result, license_verifier.Rejected):
        return REJECTION_MESSAGES.get(result.reason, "This copy cannot check licenses. Contact support.")
    if result.product is license_verifier.ProductCheck.MISMATCH:
        return "This license is for another product."
    if result.hardware is license_verifier.HardwareCheck.MISMATCH:
        return "This license is for another computer. Send us this computer's ID: %s" % hardware_id()
    if result.expiration is license_verifier.ExpirationCheck.EXPIRED:
        return "This license expired on %s." % result.license.expiration_date.isoformat()
    return "This license cannot be used on this computer."
# end of guide excerpt


# guide excerpt: python-hardware-id
def derive_hardware_id(machine_id, product_id):
    identifier = machine_id.strip(" \t\r\n")
    digest = hmac.new(product_id.encode("utf-8"), identifier.encode("utf-8"), hashlib.sha256)
    return digest.hexdigest().upper()
# end of guide excerpt


# guide excerpt: python-startup
def current_license():
    license_code = load_license_code()
    if license_code is None:
        return None
    return license_verifier.verify(license_code, PUBLIC_KEY, PRODUCT_ID, hardware_id(), datetime.now(timezone.utc))
# end of guide excerpt


class GuideExampleTests(VectorTestCase):
    def setUp(self):
        STORED_CODES.clear()

    def test_activation_stores_a_valid_code_and_explains_every_problem(self):
        valid = self.cases["legacy-bound-perpetual"]["license"]
        self.assertEqual("Activated for Alex Morgan.", activate(valid))
        problems = {
            "not-a-license": "Paste the complete license code you received.",
            "unsupported-format-version": "This license needs a newer version of the application.",
            "tampered-registration-name": "This license was not issued for this application.",
            "issued-for-another-product": "This license is for another product.",
            "bound-to-another-machine": "This license is for another computer. Send us this computer's ID: DEMO-WORKSTATION-001",
        }
        for name, message in problems.items():
            with self.subTest(vector=name):
                self.assertEqual(message, activate(self.cases[name]["license"]))
        self.assertEqual([valid], STORED_CODES)
        expired = self.inspect(self.cases["legacy-expires-at-check-date"]["license"])
        self.assertEqual("This license expired on 2027-03-15.", activation_problem(expired))
        unusable_key = verifier.Rejected(verifier.InspectionFailure.INVALID_PUBLIC_KEY)
        self.assertEqual("This copy cannot check licenses. Contact support.", activation_problem(unusable_key))
        unchecked = self.inspect(valid, hardware_id=None)
        self.assertEqual("This license cannot be used on this computer.", activation_problem(unchecked))

    def test_hardware_id_is_derived_as_published(self):
        self.assertGreater(len(self.vectors["hardwareIdExamples"]), 1)
        for example in self.vectors["hardwareIdExamples"]:
            with self.subTest(machine_id=example["machineId"]):
                derived = derive_hardware_id(example["machineId"], example["productId"])
                self.assertEqual(example["hardwareId"], derived)
                self.assertNotEqual(derived, derive_hardware_id(example["machineId"], "Other-Product"))

    def test_startup_check_verifies_the_stored_code_each_time(self):
        self.assertIsNone(current_license())
        save_license_code(self.cases["legacy-bound-perpetual"]["license"])
        self.assertEqual("Alex Morgan", current_license().registration_name)
        save_license_code(self.cases["tampered-registration-name"]["license"])
        self.assertIsNone(current_license())


if __name__ == "__main__":
    unittest.main()
