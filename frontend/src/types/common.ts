// Shared API contract types
export interface ErrorDetail {
  code: number;
  status: string;
  message: string;
  detail?: string;
}

export interface ErrorResponse {
  error: ErrorDetail;
}
