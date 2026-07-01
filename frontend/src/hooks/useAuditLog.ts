// Service layer - audit log
import { useCallback, useEffect, useState } from "react";
import { auditApi } from "../api/audit";
import { ApiError } from "../api/errors";
import type { AuditLog } from "../types/audit";

const DEFAULT_LIMIT = 50;

export function useAuditLog() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [limit, setLimit] = useState(DEFAULT_LIMIT);
  const [table, setTable] = useState("");

  const load = useCallback(async (lim: number, tbl: string) => {
    setIsLoading(true);
    setError(null);
    try {
      setLogs(await auditApi.getHistory(lim, tbl || undefined));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : ApiError.fromStatus(1).message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void load(limit, table);
  }, [load, limit, table]);

  return {
    logs, isLoading, error,
    limit, setLimit,
    table, setTable,
    refresh: () => load(limit, table),
  };
}
