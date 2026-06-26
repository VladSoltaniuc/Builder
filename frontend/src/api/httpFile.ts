// Transport layer — file API requests
import { ApiError, parseError } from './errors';

async function request<T>(path: string, method: string, file?: File, asBlob = false): Promise<T> {
  let form = new FormData();
  if (file) form.append('file', file); // wrap file for multipart/form-data transport

  let response: Response;
  try {
    response = await fetch(`${import.meta.env.VITE_API_BASE_URL}${path}`, {
      method,
      body: file ? form : undefined, // no body for DELETE/GET
    });
  } catch {
    // Server is down or no network
    throw ApiError.fromStatus(0);
  }

  if (!response.ok) {
    throw await parseError(response);
  }

  // DELETE with no body to return
  if (response.status === 204) return undefined as T;

  // download Blob (binary), everything else JSON
  return (asBlob ? response.blob() : response.json()) as Promise<T>;
}

// Interface
export const httpFile = {
  upload:   <T>(path: string, file: File) => request<T>(path,    'POST',   file), 
  replace:  <T>(path: string, file: File) => request<T>(path,    'PUT',    file),
  download:     (path: string)            => request<Blob>(path,  'GET',   undefined, true), 
  delete:       (path: string)            => request<void>(path,  'DELETE'),
};
