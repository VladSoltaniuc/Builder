// Auth layer — JWT persistence in localStorage
const TOKEN_KEY = "auth_token";

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

// Fired by the transport layer on a 401 so the AuthProvider can drop the session.
export const AUTH_LOGOUT_EVENT = "auth:logout";
