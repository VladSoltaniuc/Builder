-- Seed 10,000 products for stress testing.
-- Run against your local DB before starting the JMeter test:
--   psql -h localhost -U postgres -d productdb -f seed-10k-products.sql
--
-- Safe to re-run: names are unique and won't conflict with real data.
-- Clean up afterwards with: DELETE FROM "Products" WHERE "Name" LIKE 'StressTest-%';

INSERT INTO "Products" ("Name", "Category", "Price", "Stock", "Version")
SELECT
    'StressTest-' || gs AS "Name",
    (ARRAY['Electronics','Peripherals','Accessories','Storage','Networking'])[((gs - 1) % 5) + 1] AS "Category",
    ROUND((RANDOM() * 990 + 10)::numeric, 2) AS "Price",
    (RANDOM() * 1000)::int                   AS "Stock",
    0                                         AS "Version"
FROM generate_series(1, 10000) gs;
