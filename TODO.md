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
- [x] Registration (email verification fire and forget)
- [x] Auth
- [x] OAuth
- [x] 2FA
- [x] Roles & permissions (admin vs read-only)

## Database
- [x] Migrate from in-memory to PostgreSQL + Entity Framework Core
- [x] JWT bitmask for extra permissions

## Backend
- [x] View (virtual table from joins + compu1d columns) (daily, weekly, monthly metrics computed table)
- [x] Indexes
- [x] Reindexing
- [x] Timezone converter
- [x] DB level History
- [x] Postgress Procedure
- [x] Postgress Functions
- [x] Postgress Trigger
- [x] Execution plan
- [x] Middlewares (Error, etc.)
- [x] File upload (images, documents)
- [x] Audit logs
- [x] Email sending (cron job) (subscribe to audit logs weekly report)
- [x] SMS sending
- [x] Queue (could also be upgraded to RabbitMQ, moving from RAM to 2nd server)
- [x] WebSocket (SignalR live order-status)

## Testing & CI/CD
- [x] Unit tests
- [x] Integration tests
- [x] CI (GitHub Actions)

## Performance & Concurrency
- [x] Parallel programming
- [ ] Multithreading
- [ ] 10,000 record stress test
