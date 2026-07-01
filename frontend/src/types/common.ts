// Shared API contract types
export interface FieldError {
  field: string;
  code: string;
}

export interface ErrorDetail {
  code: number;
  status: string;
  detail?: string;
  errors?: FieldError[];
}

export interface ErrorResponse {
  error: ErrorDetail;
}
