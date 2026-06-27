-- Execution plans analyze how PostgreSQL runs a query. Here, EXPLAIN ANALYZE showed Postgres doing a full Seq Scan, reading the whole AuditLogs table and discarding ~99% of rows. Adding an index on "ChangedAt" let it skip straight to the matching rows, dropping the query from ~142 ms to a few ms.

EXPLAIN ANALYZE
SELECT "TableName", "Action", COUNT(*)
FROM "AuditLogs"
WHERE "ChangedAt" >= date_trunc('week', now()) - interval '7 days'
  AND "ChangedAt" <  date_trunc('week', now())
GROUP BY "TableName", "Action";
