// API layer - transport-level status messages (network down, 401, 5xx…),
// localized by HTTP status in the active language. Code 1 is the catch-all
import i18n from '../i18n';

export function getStatusMessage(status: number): string {
  return i18n.t(`errors.http.${status}`, { defaultValue: i18n.t('errors.http.1') });
}
