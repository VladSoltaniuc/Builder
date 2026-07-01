// API layer - error handling
import i18n from '../i18n';
import { getStatusMessage } from './errorMessages';
import type { ErrorResponse, FieldError } from '../types/common';

export class ApiError extends Error {
  static fromStatus(status: number): ApiError {
    return new ApiError(getStatusMessage(status), status);
  }

  constructor(
    message: string,
    public readonly status: number,
    public readonly errorCode: string = 'UNKNOWN',
    public readonly detail?: string,
    public readonly fieldErrors?: FieldError[],
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

// Per-field validation codes → one localized line each, in the active language
function translateFieldErrors(errors: FieldError[]): string {
  return errors
    .map((e) => i18n.t(`errors.fields.${e.code}`, { defaultValue: e.code }))
    .join('\n');
}

export async function parseError(response: Response): Promise<ApiError> {
  const body = await response.json() as ErrorResponse;
  const { status: code, detail, errors: fieldErrors } = body.error;
  // The backend sends only a stable code; we own the user-facing text. `detail`
  // carries any interpolation data (e.g. the column list for MISSING_COLUMNS)
  const message = fieldErrors?.length
    ? translateFieldErrors(fieldErrors)
    : i18n.t(`errors.${code}`, { columns: detail, defaultValue: i18n.t('errors.UNKNOWN') });
  return new ApiError(message, response.status, code, detail, fieldErrors);
}
