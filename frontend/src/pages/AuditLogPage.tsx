// Application layer
import { useTranslation } from "react-i18next";
import { useAuditLog } from "../hooks/useAuditLog";
import { formatDateTime } from "../utils/datetime";
import { ALLOWED_PAGE_SIZES } from "../constants/pagination";

// Row snapshots are opaque JSON; show a compact one-line preview per cell
function preview(data: unknown): string {
  return data == null ? "—" : JSON.stringify(data);
}

export function AuditLogPage() {
  const { t, i18n } = useTranslation();
  const { logs, isLoading, error, limit, setLimit, table, setTable } = useAuditLog();

  return (
    <main className="container">
      <header><h1>{t("audit.title")}</h1></header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="toolbar">
        <input
          placeholder={t("audit.filterTable")}
          value={table}
          onChange={(e) => setTable(e.target.value)}
        />
        <select value={limit} onChange={(e) => setLimit(Number(e.target.value))}>
          {ALLOWED_PAGE_SIZES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      {isLoading ? (
        <p className="loading">{t("common.loading")}</p>
      ) : logs.length === 0 ? (
        <p className="empty">{t("audit.empty")}</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>{t("audit.col.time")}</th>
              <th>{t("audit.col.user")}</th>
              <th>{t("audit.col.table")}</th>
              <th>{t("audit.col.action")}</th>
              <th className="num">{t("audit.col.rowId")}</th>
              <th>{t("audit.col.old")}</th>
              <th>{t("audit.col.new")}</th>
            </tr>
          </thead>
          <tbody>
            {logs.map((log) => (
              <tr key={log.id}>
                <td>{formatDateTime(log.changedAt, i18n.language)}</td>
                <td>{log.changedBy ?? "—"}</td>
                <td>{log.tableName}</td>
                <td>{log.action}</td>
                <td className="num">{log.rowId}</td>
                <td style={{ maxWidth: 280, overflowX: "auto" }}><code>{preview(log.oldData)}</code></td>
                <td style={{ maxWidth: 280, overflowX: "auto" }}><code>{preview(log.newData)}</code></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </main>
  );
}
