// API layer - shared query param builder
export function buildPagedParams(
  page: number,
  pageSize: number,
  sortBy?: string,
  search?: string,
): URLSearchParams {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (sortBy)  params.set('sortBy', sortBy);
  if (search)  params.set('search', search);
  return params;
}
