// Copyright (c) 2026 Gonzalo Martínez García.
// License Generator integration example: you may use, copy, modify and distribute this file
// under section 2 of the License Generator EULA, keeping this notice.

/**
 * Reference License Generator verifier for Node.js 20 or later.
 *
 * It uses only node:crypto and does no I/O: the caller supplies the trusted
 * public key, the expected product and hardware ID, and the current time. Its
 * results match the C# reference verifier for every published test vector.
 */
import { constants, createHash, createPublicKey, verify as verifySignature } from "node:crypto";

export const MAXIMUM_ENCODED_LENGTH = 131072;
export const MAXIMUM_JSON_BYTES = (MAXIMUM_ENCODED_LENGTH / 4) * 3;
export const MAXIMUM_JSON_DEPTH = 8;
export const MAXIMUM_PUBLIC_KEY_LENGTH = 65536;
export const MINIMUM_KEY_SIZE = 2048;
const MAX_APP_ID_LENGTH = 128;
const MAX_APP_ID_UTF8_BYTES = 255;
const MAX_REGISTRATION_NAME_LENGTH = 1024;
const MAX_HARDWARE_ID_LENGTH = 1024;
const MAX_USER_DEFINED_LENGTH = 8192;

export const InspectionFailure = Object.freeze({
  MALFORMED_LICENSE: "malformed",
  UNSUPPORTED_FORMAT: "unsupported-format",
  INVALID_EXPECTED_PRODUCT: "invalid-expected-product",
  INVALID_PUBLIC_KEY: "invalid-public-key",
  SIGNATURE_MISMATCH: "signature-mismatch",
});
export const ProductCheck = Object.freeze({ MATCH: "match", MISMATCH: "mismatch" });
export const HardwareCheck = Object.freeze({
  NOT_BOUND: "not-bound", NOT_CHECKED: "not-checked", MATCH: "match", MISMATCH: "mismatch",
});
export const ExpirationCheck = Object.freeze({ NEVER_EXPIRES: "never", CURRENT: "current", EXPIRED: "expired" });

/** Returns the license only when it is authentic and valid for this product, machine and time. */
export function verify(encodedLicense, publicKeyPem, expectedAppId, expectedHardwareId, now) {
  if (typeof expectedHardwareId !== "string") return null;
  const result = inspect(encodedLicense, publicKeyPem, expectedAppId, expectedHardwareId, now);
  return result.result === "authenticated" && result.isValid ? result.license : null;
}

/**
 * Authenticates first, then reports policy checks. Pass null as the expected
 * hardware ID to skip that check explicitly; undefined is refused, so an
 * omitted argument cannot skip it by accident.
 */
export function inspect(encodedLicense, publicKeyPem, expectedAppId, expectedHardwareId, now) {
  if (!(now instanceof Date) || Number.isNaN(now.getTime())) throw new TypeError("now must be a valid Date.");
  if (expectedHardwareId !== null && typeof expectedHardwareId !== "string") {
    throw new TypeError("expectedHardwareId must be a string, or null to skip the hardware check.");
  }
  if (typeof expectedAppId !== "string" || isBlank(expectedAppId)) return rejected(InspectionFailure.INVALID_EXPECTED_PRODUCT);
  if (typeof publicKeyPem !== "string" || isBlank(publicKeyPem) || publicKeyPem.length > MAXIMUM_PUBLIC_KEY_LENGTH) {
    return rejected(InspectionFailure.INVALID_PUBLIC_KEY);
  }
  let envelope;
  try {
    envelope = readEnvelope(encodedLicense);
  } catch (error) {
    if (error instanceof EnvelopeError) return rejected(error.failure);
    throw error;
  }
  const key = readPublicKey(publicKeyPem);
  if (key === null) return rejected(InspectionFailure.INVALID_PUBLIC_KEY);
  let authentic;
  try {
    authentic = verifySignature("sha256", envelope.signedBytes, { key, padding: constants.RSA_PKCS1_PADDING }, envelope.signature);
  } catch {
    authentic = false;
  }
  if (!authentic) return rejected(InspectionFailure.SIGNATURE_MISMATCH);

  const claims = envelope.license;
  const product = claims.appId === expectedAppId ? ProductCheck.MATCH : ProductCheck.MISMATCH;
  const hardware = claims.hardwareId.length === 0 ? HardwareCheck.NOT_BOUND
    : expectedHardwareId === null ? HardwareCheck.NOT_CHECKED
    : claims.hardwareId === expectedHardwareId ? HardwareCheck.MATCH : HardwareCheck.MISMATCH;
  const expiration = claims.expirationDate === null ? ExpirationCheck.NEVER_EXPIRES
    : now.getTime() >= Date.parse(claims.expirationDate + "T00:00:00Z") ? ExpirationCheck.EXPIRED : ExpirationCheck.CURRENT;
  const isValid = product === ProductCheck.MATCH
    && (hardware === HardwareCheck.NOT_BOUND || hardware === HardwareCheck.MATCH)
    && (expiration === ExpirationCheck.NEVER_EXPIRES || expiration === ExpirationCheck.CURRENT);
  // Authenticated claims may still fail the caller's product, hardware or expiration policy.
  return Object.freeze({ result: "authenticated", license: claims, product, hardware, expiration,
    checkedAtUtc: new Date(now.getTime()), isValid });
}

/** Checks envelope syntax and limits before key I/O. Success is not authentication and exposes no claims. */
export function checkFormat(encodedLicense) {
  try {
    readEnvelope(encodedLicense);
    return null;
  } catch (error) {
    if (error instanceof EnvelopeError) return error.failure;
    throw error;
  }
}

class EnvelopeError extends Error {
  constructor(failure) {
    super(failure);
    this.failure = failure;
  }
}

const malformed = () => new EnvelopeError(InspectionFailure.MALFORMED_LICENSE);
const rejected = (reason) => Object.freeze({ result: "rejected", reason });

// Only these characters may separate Base64 groups, as in .NET's Convert.FromBase64String.
const BASE64_WHITESPACE = /[ \t\r\n]/g;
const BASE64 = /^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/;
// The ISO 8601 profile that System.Text.Json reads as a date and time; the
// published test vectors list the texts it accepts and rejects.
const ISO_DATE_TIME = /^([0-9]{4})-([0-9]{2})-([0-9]{2})(?:T([0-9]{2}):([0-9]{2})(?::([0-9]{2})(?:\.[0-9]{1,16})?)?(?:Z|([+-])([0-9]{2})(?::([0-9]{2}))?)?)?$/;
const MAXIMUM_OFFSET_MINUTES = 14 * 60;
const PUBLIC_KEY_PEM = /^-----BEGIN (PUBLIC KEY|RSA PUBLIC KEY)-----([A-Za-z0-9+/=\t\n\r ]*)-----END \1-----$/;
// The whitespace of .NET's char.IsWhiteSpace.
const BLANK = /^[\t\n\v\f\r\x85\p{Zs}\p{Zl}\p{Zp}]*$/u;
const EDGE_WHITESPACE = /^[\t\n\v\f\r\x85\p{Zs}\p{Zl}\p{Zp}]+|[\t\n\v\f\r\x85\p{Zs}\p{Zl}\p{Zp}]+$/gu;

function readEnvelope(encodedLicense) {
  if (typeof encodedLicense !== "string" || isBlank(encodedLicense) || encodedLicense.length > MAXIMUM_ENCODED_LENGTH) {
    throw malformed();
  }
  const root = parseJson(decodeBase64(encodedLicense));

  const appId = readNonblankText(root, "AppId");
  const registrationName = readNonblankText(root, "RegistrationName");
  const hardwareId = readText(root, "HardwareId");
  const signature = readNonblankText(root, "Signature");
  let userDefined = root.get("UserDefined") ?? null;
  if (userDefined !== null) {
    if (typeof userDefined !== "string" || !userDefined.isWellFormed()) throw malformed();
    userDefined = userDefined || null;
  }
  const version = root.get("FormatVersion") ?? null;
  let formatVersion = null;
  if (version !== null) {
    if (!(version instanceof JsonNumber) || !/^-?(?:0|[1-9][0-9]*)$/.test(version.text)) throw malformed();
    formatVersion = Number(version.text);
    if (formatVersion < -(2 ** 31) || formatVersion >= 2 ** 31) throw malformed();
    if (formatVersion !== 1 && formatVersion !== 2) throw new EnvelopeError(InspectionFailure.UNSUPPORTED_FORMAT);
  }
  const expirationDate = root.has("ExpirationDate") ? readExpirationDate(root.get("ExpirationDate")) : null;

  const signatureBytes = decodeBase64(signature);
  if (signatureBytes.length === 0) throw malformed();
  // Every encoding of one license has this identifier, so a revocation list compares it, not the text.
  const revocationId = createHash("sha256").update(signatureBytes).digest("hex");
  const license = Object.freeze({ appId, registrationName, hardwareId, expirationDate, userDefined, formatVersion,
    revocationId });
  const signedBytes = Buffer.from(dataToSign(license), "utf8");
  return { license, signedBytes, signature: signatureBytes };
}

function decodeBase64(text) {
  const compact = text.replace(BASE64_WHITESPACE, "");
  if (!BASE64.test(compact)) throw malformed();
  return Buffer.from(compact, "base64");
}

function readText(root, name) {
  const value = root.get(name);
  if (typeof value !== "string" || !value.isWellFormed()) throw malformed();
  return value;
}

function readNonblankText(root, name) {
  const value = readText(root, name);
  if (isBlank(value)) throw malformed();
  return value;
}

function readExpirationDate(value) {
  if (value === null) return null;
  const match = typeof value === "string" ? ISO_DATE_TIME.exec(value) : null;
  if (match === null) throw malformed();
  const [year, month, day, hour, minute, second] = match.slice(1, 7).map((part) => Number(part ?? 0));
  const [sign, offsetHours, offsetMinutes] = [match[7], Number(match[8] ?? 0), Number(match[9] ?? 0)];
  const offset = offsetHours * 60 + offsetMinutes;
  if (offsetMinutes > 59 || offset > MAXIMUM_OFFSET_MINUTES || hour > 23 || minute > 59 || second > 59) throw malformed();
  const written = new Date(0);
  written.setUTCFullYear(year, month - 1, day);
  written.setUTCHours(hour, minute, second, 0);
  if (year < 1 || written.getUTCFullYear() !== year || written.getUTCMonth() !== month - 1 || written.getUTCDate() !== day) {
    throw malformed();
  }
  // The written instant must exist in UTC, even though only its calendar date is signed.
  const instant = written.getTime() - (sign === "-" ? -offset : offset) * 60000;
  if (instant < FIRST_INSTANT || instant > LAST_INSTANT) throw malformed();
  // Only the calendar date is signed; a timezone or time of day cannot extend it.
  return match[1] + "-" + match[2] + "-" + match[3];
}

const FIRST_INSTANT = (() => { const date = new Date(0); date.setUTCFullYear(1, 0, 1); return date.getTime(); })();
const LAST_INSTANT = Date.UTC(9999, 11, 31, 23, 59, 59);

function dataToSign(license) {
  const { appId, registrationName, hardwareId, userDefined } = license;
  if (appId.length > MAX_APP_ID_LENGTH || Buffer.byteLength(appId, "utf8") > MAX_APP_ID_UTF8_BYTES
      || registrationName.length > MAX_REGISTRATION_NAME_LENGTH || hardwareId.length > MAX_HARDWARE_ID_LENGTH
      || (userDefined !== null && userDefined.length > MAX_USER_DEFINED_LENGTH)) {
    throw malformed();
  }
  const expiration = license.expirationDate ?? "NEVER";
  if (license.formatVersion === null || license.formatVersion === 1) {
    if (userDefined !== null || [appId, registrationName, hardwareId].some((field) => field.includes("|"))) throw malformed();
    return [appId, registrationName, hardwareId, expiration].join("|");
  }
  if (userDefined === null) throw malformed();
  return "LG2|" + [appId, registrationName, hardwareId, expiration, userDefined]
    .map((field) => Buffer.from(field, "utf8").toString("base64")).join("|");
}

function readPublicKey(pem) {
  const match = PUBLIC_KEY_PEM.exec(pem.replace(EDGE_WHITESPACE, ""));
  if (match === null) return null;
  const compact = match[2].replace(BASE64_WHITESPACE, "");
  if (!BASE64.test(compact)) return null;
  const der = Buffer.from(compact, "base64");
  const type = match[1] === "PUBLIC KEY" ? "spki" : "pkcs1";
  let key;
  try {
    key = createPublicKey({ key: der, format: "der", type });
  } catch {
    return null;
  }
  // Re-encoding rejects bytes after the key and any encoding that is not DER.
  if (!key.export({ format: "der", type }).equals(der)) return null;
  if (key.asymmetricKeyType !== "rsa" || key.asymmetricKeyDetails.modulusLength < MINIMUM_KEY_SIZE) return null;
  return key;
}

function isBlank(text) {
  return BLANK.test(text);
}

/** A JSON number keeps its text, because only an integer text is a format version. */
class JsonNumber {
  constructor(text) {
    this.text = text;
  }
}

/**
 * Parses strict RFC 8259 JSON, as System.Text.Json does by default: no BOM,
 * comments or trailing commas, at most MAXIMUM_JSON_DEPTH nested containers,
 * and a root object whose member names are unique.
 */
function parseJson(bytes) {
  if (bytes.length > MAXIMUM_JSON_BYTES) throw malformed();
  let text;
  try {
    text = new TextDecoder("utf-8", { fatal: true, ignoreBOM: true }).decode(bytes);
  } catch {
    throw malformed();
  }
  const parser = new JsonParser(text);
  const root = parser.value(1);
  parser.end();
  if (!Array.isArray(root?.members)) throw malformed();
  const names = root.members.map(([name]) => name);
  if (new Set(names).size !== names.length || !names.every((name) => name.isWellFormed())) throw malformed();
  return new Map(root.members);
}

class JsonParser {
  constructor(text) {
    this.text = text;
    this.index = 0;
  }

  value(depth) {
    this.skipWhitespace();
    const character = this.text[this.index];
    if (character === "{" || character === "[") {
      if (depth > MAXIMUM_JSON_DEPTH) throw malformed();
      return character === "{" ? this.object(depth) : this.array(depth);
    }
    if (character === '"') return this.string();
    for (const [literal, value] of [["true", true], ["false", false], ["null", null]]) {
      if (this.text.startsWith(literal, this.index)) {
        this.index += literal.length;
        return value;
      }
    }
    const number = /-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?/y;
    number.lastIndex = this.index;
    const match = number.exec(this.text);
    if (match === null) throw malformed();
    this.index = number.lastIndex;
    return new JsonNumber(match[0]);
  }

  object(depth) {
    const members = [];
    this.index += 1;
    this.skipWhitespace();
    if (this.text[this.index] === "}") {
      this.index += 1;
      return { members };
    }
    for (;;) {
      this.skipWhitespace();
      if (this.text[this.index] !== '"') throw malformed();
      const name = this.string();
      this.expect(":");
      members.push([name, this.value(depth + 1)]);
      if (this.separator("}")) return { members };
    }
  }

  array(depth) {
    const items = [];
    this.index += 1;
    this.skipWhitespace();
    if (this.text[this.index] === "]") {
      this.index += 1;
      return items;
    }
    for (;;) {
      items.push(this.value(depth + 1));
      if (this.separator("]")) return items;
    }
  }

  string() {
    let result = "";
    this.index += 1;
    for (;;) {
      const character = this.text[this.index];
      if (character === undefined || character < " ") throw malformed();
      this.index += 1;
      if (character === '"') return result;
      if (character !== "\\") {
        result += character;
        continue;
      }
      const escape = this.text[this.index];
      this.index += 1;
      const simple = { '"': '"', "\\": "\\", "/": "/", b: "\b", f: "\f", n: "\n", r: "\r", t: "\t" }[escape];
      if (simple !== undefined) {
        result += simple;
      } else if (escape === "u" && /^[0-9A-Fa-f]{4}$/.test(this.text.slice(this.index, this.index + 4))) {
        result += String.fromCharCode(parseInt(this.text.slice(this.index, this.index + 4), 16));
        this.index += 4;
      } else {
        throw malformed();
      }
    }
  }

  separator(closing) {
    this.skipWhitespace();
    const character = this.text[this.index];
    this.index += 1;
    if (character === closing) return true;
    if (character === ",") return false;
    throw malformed();
  }

  expect(character) {
    this.skipWhitespace();
    if (this.text[this.index] !== character) throw malformed();
    this.index += 1;
  }

  skipWhitespace() {
    while (" \t\n\r".includes(this.text[this.index] ?? "x")) this.index += 1;
  }

  end() {
    this.skipWhitespace();
    if (this.index !== this.text.length) throw malformed();
  }
}
