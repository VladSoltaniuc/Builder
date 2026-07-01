// Formats a UTC timestamp from the API into the user's local timezone
// The API always sends UTC (ISO "...Z"); the browser knows the user's zone and
// handles DST automatically, so conversion happens entirely here at display time
export function formatDateTime(utc: string, locale?: string): string {
  return new Date(utc).toLocaleString(locale, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}
