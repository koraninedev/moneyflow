# TASKS.md — MoneyFlow Implementation Checklist

> For the implementation agent (Grok 4.6 or successor): work top-to-bottom. Each phase should build and run before moving to the next. Reference PROJECT_SPEC.md, ARCHITECTURE.md, DATABASE_DESIGN.md, API_SPEC.md, UI_UX_SPEC.md for exact contracts — do not invent schema/endpoints not listed there without updating the relevant spec file first.

## Phase 1 — Project Bootstrap
- [x] Create `/frontend` with Vite React-TS template
- [x] Install frontend deps: `antd`, `tailwindcss` (+ postcss/autoprefixer), `@tanstack/react-query`, `react-router-dom`, `zustand`, `react-hook-form`, `@hookform/resolvers`, `zod`, `recharts`, `dayjs`
- [x] Configure Tailwind (`tailwind.config.ts`, `postcss.config.js`, base `index.css` with `@tailwind` directives)
- [x] Configure AntD `ConfigProvider` theme per UI_UX_SPEC.md §2
- [x] Create `/backend/MoneyFlow.Api` ASP.NET Core Web API project (net8.0)
- [x] Add NuGet packages: `Dapper`, `Microsoft.Data.SqlClient`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `BCrypt.Net-Next`, `FluentValidation.AspNetCore`
- [x] Create `/backend/MoneyFlow.Api.Tests` xUnit test project
- [x] Create `/database/sql` folder
- [x] Create root `.gitignore` (node_modules, bin/obj, appsettings.Development.json, .env, .env.local)
- [x] Create `.env.example` (frontend) and document backend config keys in `appsettings.json` (no secrets committed)

## Phase 2 — Database
- [x] Write `database/sql/001_create_database.sql`
- [x] Write `database/sql/002_create_tables.sql` (all 10 tables from DATABASE_DESIGN.md §3, in FK-safe order, with all constraints/checks)
- [x] Write `database/sql/003_create_indexes.sql` (all indexes from DATABASE_DESIGN.md §4)
- [x] Write `database/sql/004_seed_data.sql` (demo user + categories + Sept 2026 + Aug 2026 data per DATABASE_DESIGN.md §8)
- [ ] Run all 4 scripts against a local SQL Server / LocalDB instance and confirm no errors
- [ ] Verify FK cascade behavior manually: deleting the seed user removes all its data; deleting a `MonthlyPeriod` removes only its entries

## Phase 3 — Authentication
- [x] Implement `PasswordHasher` (BCrypt wrapper)
- [x] Implement `JwtTokenService` (issue token with `sub`=UserId, `email` claim, 7-day expiry, reads secret from config)
- [x] Implement `ICurrentUserService` / `CurrentUserService` reading `UserId` from `ClaimsPrincipal`
- [x] Implement `UsersRepository` (Dapper: insert user, get by email, get by id)
- [x] Implement `AuthService` (register, login)
- [x] Implement `AuthController`: `POST /auth/register`, `POST /auth/login`, `GET /auth/me`
- [x] Wire JWT Bearer middleware in `Program.cs`; add `[Authorize]` as default policy except auth endpoints
- [x] Seed default categories for a newly registered user (same list as `004_seed_data.sql` categories, zero entries)
- [x] Configure CORS to allow the frontend dev origin
- [x] Frontend: `auth.store.ts` (zustand) + `api/auth.ts` + Login/Register pages + route guard (`ProtectedRoute`) redirecting to `/login` when no token
- [ ] Manual test: register → login → call `/auth/me` with the returned token succeeds; without token returns 401

## Phase 4 — Core Monthly Finance APIs (backend)
- [x] `CategoriesRepository` + `CategoriesService` + `CategoriesController` (CRUD per API_SPEC.md §4)
- [x] `MonthlyPeriodsRepository` + `MonthlyPeriodsService` (create Empty/CopyRecurring, get by year/month, list) + `MonthsController` (API_SPEC.md §3)
- [x] `RecurringTemplatesRepository` + `RecurringTemplatesService` (CRUD + "apply to new period" logic) + `RecurringItemsController` (API_SPEC.md §10)
- [x] `IncomeEntriesRepository` + `IncomeService` + `IncomesController` (API_SPEC.md §5)
- [x] `ExpenseEntriesRepository` + `ExpensesService` (incl. paid toggle, reorder) + `ExpensesController` (API_SPEC.md §6)
- [x] `BudgetAllocationsRepository` + `BudgetsService` + `BudgetsController` (API_SPEC.md §7)
- [x] `TransactionsRepository` + `TransactionsService` (incl. quick-add) + `TransactionsController` (API_SPEC.md §8)
- [x] `SavingGoalsRepository`, `SavingContributionsRepository` + `SavingsService` + `SavingsController` (API_SPEC.md §9)
- [x] `MonthlySummaryService` implementing exact queries from DATABASE_DESIGN.md §6 (TotalIncome, TotalExpenses, Fixed/Variable split, TotalSavings, per-category Budget/Used/Remaining, Remaining money)
- [x] `DashboardController` (`GET /dashboard`) + wire `GET /months/{id}/summary` to the same service method
- [x] `ReportsService` + `ReportsController` (trend, category-breakdown, fixed-vs-variable — API_SPEC.md §12)
- [x] Add FluentValidation validators for all request DTOs (amount > 0, required fields, category type/trackingMode cross-checks per API_SPEC.md validation notes)
- [x] Add `ExceptionHandlingMiddleware` mapping typed exceptions → 400/404/409, generic 500 otherwise
- [x] Ensure every repository query filters by the resolved `UserId` (directly or via `MonthlyPeriodId` join) — no endpoint accepts a client-supplied `userId`

## Phase 5 — Frontend Shell
- [x] Set up React Router routes + layout shells (Desktop sidebar/topbar, Mobile bottom-nav) per UI_UX_SPEC.md §3
- [x] `month.store.ts` (zustand: selected year/month) + `useCurrentMonth` hook
- [x] `api/` typed client functions per resource, wrapping `fetch` with base URL + Authorization header + envelope unwrapping
- [x] TanStack Query client setup (`lib/query-client.ts`) with sane defaults (staleTime, retry)
- [x] Shared components: `StatCard`, `MonthSwitcher`, `ResponsiveTable`, `BudgetProgressCard`, `EmptyState`, `ConfirmDeleteModal`, `QuickAddSheet`
- [x] `lib/money.ts` (THB formatting) and `lib/buddhist-era.ts` (BE display helper)

## Phase 6 — Dashboard
- [x] Build Dashboard page: KPI stat cards, Food/Reward mini progress, paid/unpaid widget, month progress bar
- [x] Integrate 4 Recharts charts per UI_UX_SPEC.md §4.2, responsive via `ResponsiveContainer`
- [x] Empty-month state with "Start Empty Month" / "Copy Recurring Items" actions calling `POST /months`
- [x] Loading skeletons + error state (toast + retry button)

## Phase 7 — Monthly Finance Management (Income & Expenses)
- [x] Income page: list (ResponsiveTable), add/edit drawer form (React Hook Form + Zod), category quick-create inline
- [x] Expenses page: filter chips, paid/unpaid switch, add/edit drawer with Classification + Due Date, reorder (desktop drag, mobile up/down buttons)
- [x] Categories page: CRUD UI grouped by type tabs

## Phase 8 — Budgets, Transactions, Savings
- [x] Budgets page: category progress cards, allocation set/edit modal
- [x] Category transaction drawer/sheet: list + inline quick-add form
- [x] Global Quick-Add (FAB/button) → bottom sheet/modal → `POST /transactions/quick-add`, success toast with Undo
- [x] Savings page: goal cards with progress, contribute quick action, new goal form
- [x] Recurring Items page: grouped list by target type, add/edit/toggle-active

## Phase 9 — Reports and Charts
- [x] Reports page: month range selector (3/6/12), trend charts (reuse Dashboard chart components), comparison table with mobile card-carousel fallback

## Phase 10 — Responsive / Mobile Polish
- [ ] Verify every screen at 360px, 768px, 1024px, 1440px widths
- [ ] Confirm ResponsiveTable correctly switches to card list at <768px on all list screens (Income, Expenses, Transactions, Reports comparison)
- [ ] Confirm FAB + bottom nav reachable and non-overlapping on all mobile screens
- [ ] Confirm numeric inputs trigger decimal keyboard on mobile
- [ ] Confirm toasts/skeletons/empty-states are implemented on every data-fetching screen, not just Dashboard

## Phase 11 — Testing
- [x] Unit test: `MonthlySummaryService` — Income=40,000, Expenses=20,000, Savings=5,000 → Remaining=15,000 (exact fixture from PROJECT_SPEC.md §5)
- [x] Unit test: BudgetAllocations amounts are excluded from `TotalExpenses`/`Remaining` (assert allocating 7,000 for Food with 0 transactions does not change Remaining)
- [x] Unit test: Category/TrackingMode validation rejects `ExpenseEntry` against a `BudgetVsActual` category and rejects `Transaction` against a `Simple` category
- [x] Unit test: "Copy Recurring Items" generates one entry per active template with due dates clipped to the new month's day count
- [ ] Integration test: register User A and User B; A creates income/expense/transaction data; assert `GET` endpoints called with B's token never return A's records (by id → 404, by list → excluded)
- [ ] Integration test: full auth flow (register → login → authorized request → 401 without token)
- [ ] Integration test: creating a duplicate `(year, month)` period returns 409
- [ ] Integration test: creating a duplicate `BudgetAllocation` for the same `(monthlyPeriodId, categoryId)` returns 409

## Phase 12 — Build Verification
- [x] `dotnet build` succeeds with zero errors/warnings treated as errors
- [x] `dotnet test` all green
- [x] `npm run build` (frontend) succeeds with zero TypeScript errors
- [x] `npm run lint` (if configured) passes
- [ ] Manual smoke test: run backend + frontend together, log in with seeded demo user, confirm Dashboard renders seeded September 2026 data matching DATABASE_DESIGN.md §8 numbers
- [ ] Confirm no secrets present in any committed file (`git grep` for connection strings/JWT secret literals)
- [x] Update README.md if any setup step changed during implementation
