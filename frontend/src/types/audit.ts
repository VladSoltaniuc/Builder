// Shared layer - audit contract types
// GET /api/audit
export interface AuditLog {
  id: number;
  tableName: string;
  action: string;
  rowId: number;
  // Row snapshot before/after the change; shape varies per table, so JSON-opaque
  oldData: unknown | null;
  newData: unknown | null;
  changedAt: string;
  changedBy: string | null;
}
