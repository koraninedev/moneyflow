# MoneyFlow

A personal monthly income / expense / savings management web app — replaces manual iPhone Notes tracking with a fast, beautiful, mobile-friendly dashboard. Multi-user backend with strict per-user data isolation; SQL Server database.

## Documentation

Read in this order before implementing:

1. [PROJECT_SPEC.md](PROJECT_SPEC.md) — what the product does, functional requirements, accounting rules
2. [ARCHITECTURE.md](ARCHITECTURE.md) — chosen stack and system design
3. [DATABASE_DESIGN.md](DATABASE_DESIGN.md) — full SQL Server schema
4. [API_SPEC.md](API_SPEC.md) — REST API contract
5. [UI_UX_SPEC.md](UI_UX_SPEC.md) — screens, navigation, visual design
6. [TASKS.md](TASKS.md) — implementation checklist/phases

## Tech Stack

- **Frontend**: React + TypeScript + Vite + Ant Design + Tailwind CSS + TanStack Query + React Router + Zustand + React Hook Form + Zod + Recharts
- **Backend**: ASP.NET Core 8 Web API + Dapper + JWT auth
- **Database**: Microsoft SQL Server

## Project Structure

```
/frontend      React SPA
/backend       ASP.NET Core Web API (MoneyFlow.Api) + tests (MoneyFlow.Api.Tests)
/database/sql  Hand-written SQL scripts (001–004, run in order)
PROJECT_SPEC.md, ARCHITECTURE.md, DATABASE_DESIGN.md, API_SPEC.md, UI_UX_SPEC.md, TASKS.md
```

## Prerequisites

- Node.js 20+
- .NET 8 SDK
- SQL Server (LocalDB, Developer Edition, or Azure SQL) reachable from your machine

## Setup

### 1. Database

```powershell
sqlcmd -S <your-server> -i database/sql/001_create_database.sql
sqlcmd -S <your-server> -i database/sql/002_create_tables.sql
sqlcmd -S <your-server> -i database/sql/003_create_indexes.sql
sqlcmd -S <your-server> -i database/sql/004_seed_data.sql
```

### 2. Backend

```powershell
cd backend/MoneyFlow.Api
copy appsettings.Development.json.example appsettings.Development.json
# edit appsettings.Development.json: ConnectionStrings:MoneyFlowDb, Jwt:Secret
dotnet restore
dotnet run
```

Backend runs at `http://localhost:5000` by default.

### 3. Frontend

```powershell
cd frontend
copy .env.example .env
# edit .env: VITE_API_BASE_URL=http://localhost:5000/api
npm install
npm run dev
```

Frontend runs at `http://localhost:5173` by default.

Demo login after seed: `demo@moneyflow.app` / `Demo123!`

If SQL Server Express is installed but **Stopped**, start it as Administrator (`Start-Service MSSQL$SQLEXPRESS`) then run the four `sqlcmd` scripts. Do not skip this — the API needs `MoneyFlowDb`.

## Running Tests

```powershell
cd backend/MoneyFlow.Api.Tests
dotnet test
```

## Environment Variables

| Location | Key | Purpose |
|---|---|---|
| backend `appsettings.Development.json` (gitignored) | `ConnectionStrings:MoneyFlowDb` | SQL Server connection string |
| backend `appsettings.Development.json` (gitignored) | `Jwt:Secret` | Symmetric key for signing JWTs |
| backend config | `Cors:AllowedOrigins` | Allowed frontend origin(s) |
| frontend `.env` (gitignored) | `VITE_API_BASE_URL` | Base URL of the backend API |

Never commit real secrets — only `.env.example` / `appsettings.Development.json.example` files are tracked in git.

## Security Notes

- Passwords hashed with BCrypt; never stored/logged in plaintext.
- All SQL is parameterized (Dapper named parameters) — no string concatenation.
- JWT required on every endpoint except `/auth/login` and `/auth/register`; the authenticated user id is always resolved from the token, never from client input.
- Every financial table is scoped to a user (directly or via `MonthlyPeriodId`); cross-user access always returns `404`.

## License

Personal project — no license specified.
