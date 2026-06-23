# Product Catalog — CRUD Best Practice Demo

Full-stack CRUD app: React + .NET 8 Web API.

---

## Backend Architecture

### ✓ Important — learn these

- **Contracts(DTOs)** — data contracts. Define exactly what shape data must have going in and out of the API. *(Application layer)*
- **Controllers** — the entry point between frontend and backend. Minimal code — routes requests to services and returns the response. *(Presentation layer)*
- **Data** — database context (`AppDbContext`). Connects EF Core to the database and exposes tables as queryable collections. *(Infrastructure layer)*
- **Models** — business entities. The raw data model used internally, never exposed directly. *(Domain layer)*
- **Services** — business logic. Everything the app can do lives here. *(Application layer)*
- **appsettings.json** — app configuration. Holds environment-specific values like connection strings and feature flags. Never hardcode these in code, place them here.
- **Program.cs** — startup wiring. Registers services, middleware, and CORS so the app knows how to run. *(Composition root)*

### ⚠ Ignore — auto-generated or scaffolding

- **bin/** — compiled output produced by `dotnet build`. Never edit.
- **obj/** — intermediate build files produced by `dotnet build`. Never edit.
- **Properties/** — dev launch profiles (port, environment). Only relevant for local debugging.
- **ProductApi.csproj** — project definition file. Managed by the framework — only touch it to add/remove NuGet packages.


## Frontend Architecture (WIP)


## Shared

- **.gitignore** — tells Git which files not to track. Excludes auto-generated folders (`bin/`, `obj/`, `node_modules/`, `dist/`) — these can be hundreds of megabytes and are always rebuilt locally, so committing them would bloat the repository for no reason. If you ever randomly see 10.000 file changes in your git commit, this is likely the culprit.
- **README.md** — this file. Project documentation for anyone new to the codebase.
- **.claude/** — Claude Code configuration. Contains project-specific instructions for the AI assistant.

---

## Folder Structure

```
Builder/
├── backend/
│   ├── bin/                # Auto-generated — ⚠ ignore
│   ├── Contracts/          # Application layer — ✓ important
│   ├── Controllers/        # Presentation layer — ✓ important
│   ├── Data/               # Infrastructure layer — ✓ important
│   ├── Models/             # Domain layer — ✓ important
│   ├── obj/                # Auto-generated — ⚠ ignore
│   ├── Properties/         # Dev launch profiles - ⚠ ignore
│   ├── Services/           # Application layer — ✓ important
│   ├── appsettings.json    # App configuration — ✓ important
│   ├── ProductApi.csproj   # Project definition — ⚠ ignore 
│   └── Program.cs          # Composition root — ✓ important
│
└── frontend/
    ├── src/
    │   ├── api/            # Transport + API layers — HTTP calls, error handling
    │   ├── hooks/          # Service layer — data fetching, pagination, mutations
    │   ├── components/     # Presentation layer — form and table
    │   ├── types/          # TypeScript types
    │   ├── constants/      # Shared labels and validation messages
    │   └── App.tsx         # UI orchestration layer
    ├── index.html          # Shell HTML — Vite injects the built JS here
    ├── package.json        # npm dependencies and scripts
    ├── vite.config.ts      # Vite bundler config
    └── tsconfig.json       # TypeScript compiler config
```
