// Shared layer - query types
export type SortDir = 'ASC' | 'DESC';

export interface SortState {
  field: string;
  dir: SortDir;
}

export function toSortBy(sort: SortState | null): string | undefined {
  return sort ? `${sort.field}:${sort.dir}` : undefined;
}

export function toggleSort(current: SortState | null, field: string): SortState | null {
  if (current?.field !== field) return { field, dir: 'ASC' };
  if (current.dir === 'ASC') return { field, dir: 'DESC' };
  return null;
}
