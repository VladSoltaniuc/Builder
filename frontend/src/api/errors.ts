// API layer — error handling
import { getStatusMessage } from './errorMessages';
import type { ErrorResponse } from '../types/common';

export class ApiError extends Error {
  static fromStatus(status: number): ApiError {
    return new ApiError(getStatusMessage(status), status);
  }

  constructor(
    message: string,
    public readonly status: number,
    public readonly errorCode: string = 'UNKNOWN',
    public readonly detail?: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export async function parseError(response: Response): Promise<ApiError> {
  const body = await response.json() as ErrorResponse;
  return new ApiError(body.error.message, response.status, body.error.status, body.error.detail);
}
