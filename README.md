# FullStack Training Project

A full-stack sandbox application built with **.NET 8**, **React**, and **PostgreSQL**, designed not around a real product, but around learning. The domain is a simple order/product/user CRUD, but that's just the excuse to implement and understand the things that actually matter in production systems.

---

## What's inside

| Area | Details |
|---|---|
| **Frontend** | React + Material UI, i18n (EN/RO), theme switching, filtering/sorting/search, toast notifications, Excel import/export |
| **Auth** | Registration with email verification, OAuth, 2FA, role-based access (Admin vs Operator), JWT + bitmask for granular per-user permissions |
| **Backend** | Middlewares, file uploads, audit logs, email/SMS sending, in-process queue (Channel), real-time order status via SignalR |
| **Database** | PostgreSQL + EF Core - views, stored procedures, functions, triggers, indexes + reindexing, execution plan analysis, DB-level history, computed metrics table |
| **Testing & CI/CD** | Unit tests, integration tests against a real DB, GitHub Actions pipeline |

---

## Coming next

- Parallel programming & multithreading
- 10,000-record stress test

---

## Why this exists

Most tutorial projects stop at CRUD. This one uses CRUD as a foundation to go deeper, into the parts of a system that are invisible until something breaks in production.
