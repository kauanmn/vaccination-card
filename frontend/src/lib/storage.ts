import type { Role } from "./types";

export interface AuthSession {
  token: string;
  role: Role;
  expiresAt: string;
  name: string;
  username: string;
  /** null para o admin (não é um paciente). */
  patientId: string | null;
}

const KEY = "vaccination-card.session";

export function loadSession(): AuthSession | null {
  const raw = localStorage.getItem(KEY);
  if (!raw) return null;

  try {
    const session = JSON.parse(raw) as AuthSession;
    if (new Date(session.expiresAt).getTime() <= Date.now()) {
      clearSession();
      return null;
    }
    return session;
  } catch {
    clearSession();
    return null;
  }
}

export function saveSession(session: AuthSession): void {
  localStorage.setItem(KEY, JSON.stringify(session));
}

export function clearSession(): void {
  localStorage.removeItem(KEY);
}
