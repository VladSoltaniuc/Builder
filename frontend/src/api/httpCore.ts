// Transport layer - Core API requests
import { ApiError, parseError } from './errors';
import { getToken, clearToken, AUTH_LOGOUT_EVENT } from '../auth/token';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();

  let response: Response;
  try {
    response = await fetch(`${import.meta.env.VITE_API_BASE_URL}${path}`, {
      ...options, // method, body, etc.
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(options.headers ?? {}),
      },
    });
  } catch {
    // Server is down or no network
    throw ApiError.fromStatus(0);
  }

  // Token missing/expired - drop the session and let the AuthProvider redirect
  if (response.status === 401) {
    clearToken();
    window.dispatchEvent(new Event(AUTH_LOGOUT_EVENT));
    throw ApiError.fromStatus(401);
  }

  if (!response.ok) {
    throw await parseError(response);
  }

  // DELETE, PUT, POST with no body to return
  if (response.status === 204) {
    return undefined as T;
  }

  // GET return JSON as T
  return response.json() as Promise<T>;
}

// Interface
export const httpCore = {
  get:    <T>(path: string)                => request<T>(path),
  post:   <T>(path: string, body: object)  => request<T>(path, { method: 'POST',   body: JSON.stringify(body) }),
  put:    <T>(path: string, body: object)  => request<T>(path, { method: 'PUT',    body: JSON.stringify(body) }),
  delete:    (path: string)                => request<void>(path, { method: 'DELETE' }),
};
