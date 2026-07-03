import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import type { ReactNode } from "react";
import { login as loginRequest } from "../api/auth";
import { UNAUTHORIZED_EVENT } from "../lib/api";
import { decodeTokenClaims } from "../lib/jwt";
import { clearSession, loadSession, saveSession } from "../lib/storage";
import type { AuthSession } from "../lib/storage";

interface AuthContextValue {
  session: AuthSession | null;
  isAdmin: boolean;
  login: (username: string, password: string) => Promise<AuthSession>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(() =>
    loadSession(),
  );

  const logout = useCallback(() => {
    clearSession();
    setSession(null);
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    const response = await loginRequest(username, password);
    const claims = decodeTokenClaims(response.token);

    const newSession: AuthSession = {
      token: response.token,
      role: response.role,
      expiresAt: response.expiresAt,
      name: claims.name || username,
      username: claims.username || username,
      patientId: claims.patientId,
    };

    saveSession(newSession);
    setSession(newSession);
    return newSession;
  }, []);

  useEffect(() => {
    const onUnauthorized = () => setSession(null);
    window.addEventListener(UNAUTHORIZED_EVENT, onUnauthorized);
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, onUnauthorized);
  }, []);

  useEffect(() => {
    if (!session) return;
    const remaining = new Date(session.expiresAt).getTime() - Date.now();
    if (remaining <= 0) {
      logout();
      return;
    }
    const timer = setTimeout(logout, remaining);
    return () => clearTimeout(timer);
  }, [session, logout]);

  return (
    <AuthContext.Provider
      value={{ session, isAdmin: session?.role === "Admin", login, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);
  if (!value)
    throw new Error("useAuth deve ser usado dentro de <AuthProvider>");
  return value;
}
