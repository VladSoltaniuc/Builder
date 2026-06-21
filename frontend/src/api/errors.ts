// Error handling layer
import { getStatusMessage } from './errorMessages';

export class ApiError extends Error {
  static fromStatus(status: number): ApiError {
    return new ApiError(getStatusMessage(status), status);
  }

  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}
