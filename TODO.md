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
- [x] Data import/export via Excel
- [x] IP-based rate limiter

## Auth
- [x] Auth
- [x] OAuth
- [x] 2FA
- [x] Roles & permissions (admin vs read-only)

## Database
- [x] Migrate from in-memory to PostgreSQL + Entity Framework Core
- [ ] Redis (caching)

## Backend
- [ ] View (virtual table from joins + computed columns) (daily, weekly, monthly metrics computed table)
- [x] Indexes
- [X] Reindexing
- [X] Timezone converter
- [X] DB level History
- [ ] Postgress Procedure
- [X] Postgress Functions
- [X] Postgress Trigger
- [ ] Execution plan
- [ ] Middlewares (Error, etc.)
- [ ] Queue
- [ ] WebSocket chat (real-time messaging, like WhatsApp)
- [x] File upload (images, documents)
- [ ] Audit logs (needs userId after Auth implementation)
- [ ] Email sending (cron job) (subscribe to audit logs weekly report)
- [ ] SMS sending

## Testing & CI/CD
- [x] Unit tests
- [x] Integration tests
- [ ] CI/CD pipeline (GitHub Actions)

## Performance & Concurrency
- [ ] Parallel programming
- [ ] Multithreading
- [ ] 10,000 record stress test
