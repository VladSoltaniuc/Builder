// Transport layer — Core API requests
import { ApiError } from './errors';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${import.meta.env.VITE_API_BASE_URL}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...options, // method, body, etc.
    });
  } catch {
    // Server is down or no network
    throw ApiError.fromStatus(0);
  }

  if (!response.ok) {
    // 4xx / 5xx - map to user-friendly error
    throw ApiError.fromStatus(response.status);
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
