import { request } from "../lib/api";
import type { LoginResponse } from "../lib/types";

export function login(username: string, password: string): Promise<LoginResponse> {
  return request<LoginResponse>("/auth/login", {
    method: "POST",
    body: { username, password },
  });
}
