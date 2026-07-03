const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

const SUBJECT_CLAIMS = [
  "nameid",
  "sub",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
];

const NAME_CLAIMS = [
  "unique_name",
  "name",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
];

export interface TokenClaims {
  /** Id do paciente; null para o admin. */
  patientId: string | null;
  name: string;
  username: string;
}

export function decodeTokenClaims(token: string): TokenClaims {
  const payload = decodePayload(token);

  const subject = firstClaim(payload, SUBJECT_CLAIMS) ?? EMPTY_GUID;
  const name = firstClaim(payload, NAME_CLAIMS) ?? "";
  const username = typeof payload["username"] === "string" ? payload["username"] : "";

  return {
    patientId: subject === EMPTY_GUID ? null : subject,
    name,
    username,
  };
}

function decodePayload(token: string): Record<string, unknown> {
  try {
    const base64url = token.split(".")[1] ?? "";
    const base64 = base64url.replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
        .join(""),
    );
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return {};
  }
}

function firstClaim(payload: Record<string, unknown>, keys: string[]): string | null {
  for (const key of keys) {
    const value = payload[key];
    if (typeof value === "string" && value.length > 0) return value;
  }
  return null;
}
