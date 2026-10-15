// Copyright (c) 2026 Gonzalo Martínez García.
// License Generator integration example: you may use, copy, modify and distribute this file
// under section 2 of the License Generator EULA, keeping this notice.

// Checks the Node.js reference verifier against the published test vectors:
//   node --test license-verifier.test.mjs    (from this directory)
import assert from "node:assert/strict";
import { createHmac, generateKeyPairSync } from "node:crypto";
import { readFileSync } from "node:fs";
import { describe, it } from "node:test";

import * as verifier from "./license-verifier.mjs";
import { ExpirationCheck, HardwareCheck, InspectionFailure, ProductCheck, inspect, verify } from "./license-verifier.mjs";

const vectors = JSON.parse(readFileSync(new URL("../test-vectors.json", import.meta.url), "utf8"));
const cases = new Map(vectors.cases.map((vector) => [vector.name, vector]));
const now = new Date(vectors.checkedAtUtc);
const MALFORMED = { result: "rejected", reason: "malformed" };

/** The published, language-neutral form of an inspection result. */
function published(result) {
  if (result.result === "rejected") return { result: "rejected", reason: result.reason };
  const claims = result.license;
  return {
    result: "authenticated", valid: result.isValid, product: result.product, hardware: result.hardware,
    expiration: result.expiration, formatVersion: claims.formatVersion ?? 1, appId: claims.appId,
    registrationName: claims.registrationName,
    hardwareId: claims.hardwareId, expirationDate: claims.expirationDate, userDefined: claims.userDefined,
    revocationId: claims.revocationId,
  };
}

function inspectVector(license, { publicKey = vectors.publicKey, hardwareId = vectors.expectedHardwareId, at = now } = {}) {
  return verifier.inspect(license, publicKey, vectors.expectedAppId, hardwareId, at);
}

/** A case's inputs: its own key, product or hardware ID where it sets one, a null hardware ID included. */
function caseInputs(vector) {
  return [vector.license, vector.publicKey ?? vectors.publicKey, vector.expectedAppId ?? vectors.expectedAppId,
    "expectedHardwareId" in vector ? vector.expectedHardwareId : vectors.expectedHardwareId, now];
}

/** A vector's license with its JSON text edited, as an attacker or a broken client could send it. */
function edited(name, replace, withText) {
  const text = Buffer.from(cases.get(name).license, "base64").toString("utf8");
  assert.equal(text.split(replace).length - 1, 1);
  return Buffer.from(text.replace(replace, () => withText), "utf8").toString("base64");
}

describe("published vectors", () => {
  it("are present", () => assert.ok(cases.size > 0 && vectors.expirationTexts.accepted.length > 0));
  for (const [name, vector] of cases) {
    it(`gives the published result for ${name}`, () => assert.deepEqual(published(verifier.inspect(...caseInputs(vector))), vector.expected));
    it(`verify returns the license of ${name} only when it is valid and the hardware is expected`, () => {
      const inputs = caseInputs(vector);
      const license = verifier.verify(...inputs);
      if (vector.expected.valid && inputs[3] !== null) assert.equal(license.registrationName, vector.expected.registrationName);
      else assert.equal(license, null);
    });
  }
  it("checks the format without a key", () => {
    assert.equal(verifier.checkFormat(cases.get("format2-user-defined").license), null);
    assert.equal(verifier.checkFormat("This is not a license."), verifier.InspectionFailure.MALFORMED_LICENSE);
    assert.equal(verifier.checkFormat(cases.get("unsupported-format-version").license), verifier.InspectionFailure.UNSUPPORTED_FORMAT);
  });
});

describe("published expiration texts", () => {
  const texts = vectors.expirationTexts;
  const expectations = [...texts.accepted.map((text) => [text, cases.get(texts.source).expected]),
    ...texts.rejected.map((text) => [text, MALFORMED])];
  for (const [text, expected] of expectations) {
    it(`reads ${JSON.stringify(text)} as declared`, () => {
      const license = edited(texts.source, JSON.stringify(texts.written), JSON.stringify(text));
      assert.deepEqual(published(inspectVector(license)), expected);
      if (expected === MALFORMED) return;
      assert.equal(inspectVector(license, { at: new Date(texts.boundary.currentAt) }).expiration, verifier.ExpirationCheck.CURRENT);
      assert.equal(inspectVector(license, { at: new Date(texts.boundary.expiredAt) }).expiration, verifier.ExpirationCheck.EXPIRED);
    });
  }
});

// The rules the shared vectors cannot carry: private keys, whitespace around keys and JavaScript's own arguments.
describe("caller input", () => {
  const bound = cases.get("legacy-bound-perpetual");
  const rejectedKeys = {
    "blank": " ",
    "private key": generateKeyPairSync("rsa", { modulusLength: 2048 }).privateKey.export({ format: "pem", type: "pkcs8" }),
    "two blocks": vectors.publicKey + "\n" + vectors.publicKey,
    "oversized": "-----BEGIN PUBLIC KEY-----\n" + "A".repeat(verifier.MAXIMUM_PUBLIC_KEY_LENGTH),
  };
  for (const [name, pem] of Object.entries(rejectedKeys)) {
    it(`rejects a public key with ${name}`, () => {
      assert.deepEqual(inspectVector(bound.license, { publicKey: pem }), { result: "rejected", reason: verifier.InspectionFailure.INVALID_PUBLIC_KEY });
    });
  }

  it("accepts whitespace around the public key", () => {
    assert.deepEqual(published(inspectVector(bound.license, { publicKey: "\n  " + vectors.publicKey + "  \n" })), bound.expected);
  });

  it("counts the public key length in UTF-16 code units before reading the license", () => {
    const oneUnitTooLong = "\u{1F600}" + " ".repeat(verifier.MAXIMUM_PUBLIC_KEY_LENGTH - 1);
    assert.deepEqual(inspectVector("This is not a license.", { publicKey: oneUnitTooLong }),
      { result: "rejected", reason: verifier.InspectionFailure.INVALID_PUBLIC_KEY });
  });

  it("refuses an omitted hardware ID", () => {
    assert.throws(() => verifier.inspect(bound.license, vectors.publicKey, vectors.expectedAppId, undefined, now), TypeError);
  });

  it("requires a valid clock", () => {
    assert.throws(() => inspectVector(bound.license, { at: new Date("not a date") }), TypeError);
    assert.throws(() => inspectVector(bound.license, { at: Date.now() }), TypeError);
  });

  it("expires at UTC midnight of the signed date", () => {
    const license = cases.get("legacy-expires-next-day").license;
    assert.equal(inspectVector(license, { at: new Date("2027-03-15T23:59:59Z") }).expiration, verifier.ExpirationCheck.CURRENT);
    assert.equal(inspectVector(license, { at: new Date("2027-03-15T18:59:59-05:00") }).expiration, verifier.ExpirationCheck.CURRENT);
    const later = inspectVector(license, { at: new Date("2027-03-15T19:00:00-05:00") });
    assert.equal(later.expiration, verifier.ExpirationCheck.EXPIRED);
    assert.equal(later.checkedAtUtc.toISOString(), "2027-03-16T00:00:00.000Z");
  });
});

describe("license text", () => {
  it("limits the encoded size", () => {
    assert.deepEqual(inspectVector("A".repeat(verifier.MAXIMUM_ENCODED_LENGTH + 4)), MALFORMED);
    assert.deepEqual(inspectVector(" \r\n"), MALFORMED);
  });

  it("accepts line breaks inside the Base64 text", () => {
    const license = cases.get("legacy-bound-perpetual").license;
    assert.deepEqual(published(inspectVector(license.match(/.{1,76}/g).join("\r\n"))), cases.get("legacy-bound-perpetual").expected);
  });
});

// The integration guide quotes the marked regions below. They use what an
// application supplies, here taken from the published test vectors.
const PUBLIC_KEY = vectors.publicKey;
const PRODUCT_ID = vectors.expectedAppId;
const storedCodes = [];
const hardwareId = () => vectors.expectedHardwareId;
const saveLicenseCode = (licenseCode) => storedCodes.push(licenseCode);
const loadLicenseCode = () => storedCodes.at(-1) ?? null;

// guide excerpt: javascript-activation
export function activate(licenseCode) {
  const result = inspect(licenseCode, PUBLIC_KEY, PRODUCT_ID, hardwareId(), new Date());
  if (result.result === "authenticated" && result.isValid) {
    saveLicenseCode(licenseCode);
    return `Activated for ${result.license.registrationName}.`;
  }
  return activationProblem(result);
}

const REJECTION_MESSAGES = new Map([
  [InspectionFailure.MALFORMED_LICENSE, "Paste the complete license code you received."],
  [InspectionFailure.UNSUPPORTED_FORMAT, "This license needs a newer version of the application."],
  [InspectionFailure.SIGNATURE_MISMATCH, "This license was not issued for this application."],
]);

export function activationProblem(result) {
  if (result.result === "rejected") {
    return REJECTION_MESSAGES.get(result.reason) ?? "This copy cannot check licenses. Contact support.";
  }
  if (result.product === ProductCheck.MISMATCH) return "This license is for another product.";
  if (result.hardware === HardwareCheck.MISMATCH) {
    return `This license is for another computer. Send us this computer's ID: ${hardwareId()}`;
  }
  if (result.expiration === ExpirationCheck.EXPIRED) {
    return `This license expired on ${result.license.expirationDate}.`;
  }
  return "This license cannot be used on this computer.";
}
// end of guide excerpt

// guide excerpt: javascript-hardware-id
export function deriveHardwareId(machineId, productId) {
  const identifier = machineId.replace(/^[ \t\r\n]+|[ \t\r\n]+$/g, "");
  return createHmac("sha256", productId).update(identifier).digest("hex").toUpperCase();
}
// end of guide excerpt

// guide excerpt: javascript-startup
export function currentLicense() {
  const licenseCode = loadLicenseCode();
  if (licenseCode === null) return null;
  return verify(licenseCode, PUBLIC_KEY, PRODUCT_ID, hardwareId(), new Date());
}
// end of guide excerpt

describe("integration guide examples", () => {
  it("activation stores a valid code and explains every problem", () => {
    storedCodes.length = 0;
    const valid = cases.get("legacy-bound-perpetual").license;
    assert.equal(activate(valid), "Activated for Alex Morgan.");
    const problems = {
      "not-a-license": "Paste the complete license code you received.",
      "unsupported-format-version": "This license needs a newer version of the application.",
      "tampered-registration-name": "This license was not issued for this application.",
      "issued-for-another-product": "This license is for another product.",
      "bound-to-another-machine": "This license is for another computer. Send us this computer's ID: DEMO-WORKSTATION-001",
    };
    for (const [name, message] of Object.entries(problems)) assert.equal(activate(cases.get(name).license), message, name);
    assert.deepEqual(storedCodes, [valid]);
    assert.equal(activationProblem(inspectVector(cases.get("legacy-expires-at-check-date").license)), "This license expired on 2027-03-15.");
    assert.equal(activationProblem({ result: "rejected", reason: InspectionFailure.INVALID_PUBLIC_KEY }),
      "This copy cannot check licenses. Contact support.");
    assert.equal(activationProblem(inspectVector(valid, { hardwareId: null })), "This license cannot be used on this computer.");
  });

  it("derives the published hardware IDs", () => {
    assert.ok(vectors.hardwareIdExamples.length > 1);
    for (const example of vectors.hardwareIdExamples) {
      const derived = deriveHardwareId(example.machineId, example.productId);
      assert.equal(derived, example.hardwareId, JSON.stringify(example.machineId));
      assert.notEqual(deriveHardwareId(example.machineId, "Other-Product"), derived);
    }
  });

  it("startup check verifies the stored code each time", () => {
    storedCodes.length = 0;
    assert.equal(currentLicense(), null);
    saveLicenseCode(cases.get("legacy-bound-perpetual").license);
    assert.equal(currentLicense().registrationName, "Alex Morgan");
    saveLicenseCode(cases.get("tampered-registration-name").license);
    assert.equal(currentLicense(), null);
  });
});
