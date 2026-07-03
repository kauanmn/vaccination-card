import { clearSession, loadSession } from "./storage";
import type { ApiErrorDetail } from "./types";

const BASE_URL: string = import.meta.env.VITE_API_URL ?? "/api";

/** Disparado quando a API responde 401 com sessão ativa (token expirado/revogado). */
export const UNAUTHORIZED_EVENT = "vaccination-card:unauthorized";

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string,
    message: string,
    readonly details: ApiErrorDetail[] = [],
  ) {
    super(message);
    this.name = "ApiError";
  }

  /** Mensagem do detalhe de validação de um campo, se houver. */
  fieldError(field: string): string | undefined {
    return this.details.find((d) => d.field.toLowerCase() === field.toLowerCase())?.message;
  }
}

interface RequestOptions {
  method?: string;
  body?: unknown;
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const session = loadSession();
  const headers: Record<string, string> = {};

  if (options.body !== undefined) headers["Content-Type"] = "application/json";
  if (session) headers["Authorization"] = `Bearer ${session.token}`;

  let response: Response;
  try {
    response = await fetch(`${BASE_URL}${path}`, {
      method: options.method ?? "GET",
      headers,
      body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    });
  } catch {
    throw new ApiError(0, "NETWORK_ERROR", "Não foi possível conectar à API. Verifique se o servidor está rodando.");
  }

  if (response.status === 204) return undefined as T;

  let envelope: unknown;
  try {
    envelope = await response.json();
  } catch {
    throw new ApiError(response.status, "INVALID_RESPONSE", "Resposta inesperada do servidor.");
  }

  const parsed = envelope as {
    success?: boolean;
    data?: T;
    error?: { code?: string; message?: string; details?: ApiErrorDetail[] };
  };

  if (response.ok && parsed.success) return parsed.data as T;

  if (response.status === 401 && session) {
    clearSession();
    window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
  }

  throw new ApiError(
    response.status,
    parsed.error?.code ?? "UNKNOWN_ERROR",
    parsed.error?.message ?? "Erro inesperado. Tente novamente.",
    parsed.error?.details ?? [],
  );
}
