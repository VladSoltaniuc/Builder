# TODO

## UI
- [x] Add language selector EN/RO
- [x] Add another page
- [x] Filtering
- [x] Sorting
- [x] Search
- [x] Toast notifications
- [x] Theme selector
- [x] Migrate to Material UI (MUI)
- [ ] Data import/export via Excel — **~2–3 days** *(backend: ClosedXML/EPPlus; frontend: file download trigger)*
- [x] IP-based rate limiter

## Auth
- [ ] Auth — **~3–5 days** *(JWT tokens, claims, middleware, protected endpoints, frontend token storage)*
- [ ] OAuth — **~1 week** *(social login providers, redirect flows, callback handling)*
- [ ] 2FA — **~3–5 days** *(TOTP algorithm, QR code generation, authenticator app integration)*
- [ ] Roles & permissions (admin vs read-only) — **~2–3 days** *(`[Authorize(Roles="Admin")]`, frontend route guards, role seeding)*

## Database
- [x] Migrate from in-memory to PostgreSQL + Entity Framework Core
- [ ] Redis (caching) — **~3–5 days** *(install Redis, StackExchange.Redis, caching patterns: aside-cache, TTL, invalidation)*

## Backend
- [ ] View (virtual table from joins + computed columns) — **~1 day** *(PostgreSQL view SQL + EF Core keyless entity — straightforward)*
- [ ] Stored procedure + execution plans — **~2–3 days** *(write SP in Postgres, call from EF Core, read EXPLAIN ANALYZE output)*
- [ ] Indexes (reindexing, cardinality) — **~2–3 days** *(understanding query planners, index types, when to index)*
- [ ] Queue — **~1 week** *(.NET Channels for in-process, or RabbitMQ for external — new infra concepts)*
- [ ] WebSocket chat (real-time messaging, like WhatsApp) — **~1 week** *(SignalR hubs, real-time frontend connection, group messaging)*
- [ ] File upload (images, documents) — **~1–2 days** *(`IFormFile`, multipart, storage strategy: disk/S3)*
- [ ] Email sending (cron job) — **~2–3 days** *(`IHostedService` + cron expression + MailKit/SMTP)*
- [ ] SMS sending — **~1 day** *(Twilio SDK — mostly just API calls)*

## Testing & CI/CD
- [x] Unit tests
- [x] Integration tests
- [ ] CI/CD pipeline (GitHub Actions) — **~2–3 days** *(YAML workflow files, runners, secrets, build/test/deploy steps)*

## Performance & Concurrency
- [ ] Parallel programming — **~2–3 days** *(`Task.WhenAll`, `Parallel.ForEach`, PLINQ — understanding when parallelism helps vs hurts)*
- [ ] Multithreading — **~3–5 days** *(`lock`, `SemaphoreSlim`, `ConcurrentDictionary` — thread safety is subtle and easy to get wrong)*
- [ ] 10,000 record stress test — **~1–2 days** *(k6 or NBomber tool, writing load scripts, reading results)*
