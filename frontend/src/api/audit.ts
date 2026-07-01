// API layer - audit
import { httpCore } from "./httpCore";
import type { AuditLog } from "../types/audit";

const RESOURCE = "/audit";

export const auditApi = {
  getHistory: (limit: number, table?: string) => {
    const params = new URLSearchParams({ limit: String(limit) });
    if (table) params.set("table", table);
    return httpCore.get<AuditLog[]>(`${RESOURCE}?${params}`);
  },
};
