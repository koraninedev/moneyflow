# PROJECT_SPEC.md — MoneyFlow (Personal Monthly Income / Expense / Savings Manager)

> Status: Architecture phase complete. This document is the source of truth for product scope. Implementation agents must not invent requirements beyond what is written here without updating this file first.

## 1. Product Summary

MoneyFlow is a personal finance web app that replaces manual iPhone Notes tracking of monthly income, expenses, and savings. It is built for a single primary user but the backend/database is multi-tenant: every table is scoped by `UserId`, and no user can ever read or write another user's data.

The app organizes money by **Monthly Period** (e.g., September 2026), and inside each period tracks:
- Income (salary, bonus, freelance, other, custom)
- Expenses — two flavors:
  - **Simple expenses**: one-off/fixed bills entered directly (rent, phone, internet, credit cards, loans)
  - **Budget-vs-Actual categories**: a monthly allocation (e.g., Food = 7,000 THB) plus individual actual transactions logged against it (groceries week 1, week 2, ...)
- Savings goals with monthly contributions
- Reward money (self-reward budget) — modeled exactly like a Budget-vs-Actual category

## 2. Priorities (ranked, as given by product owner)

1. Extremely easy to use
2. UX friendly
3. Beautiful modern UI
4. Fast and smooth
5. Responsive and excellent on mobile
6. Simple login (no OAuth/enterprise auth)
7. Complex financial logic hidden behind a simple UI
8. SQL Server database (mandatory)
9. Maintainable architecture
10. Easy for one developer to maintain

These priorities should break ties whenever a design tradeoff appears during implementation: favor simplicity and UX over feature completeness.

## 3. Real-World Use Case (source of truth for sample data)

The user currently tracks money like this in Notes (Buddhist Era date `25-09-2569` = Gregorian `2026-09-25`):

| Item (Thai) | English | Amount (THB) | Type |
|---|---|---|---|
| ค่าห้อง | Rent | 7,200.00 | Variable expense |
| เติมเน็ตโทรศัพท์ | Phone internet top-up | 250.00 | Fixed expense |
| จ่ายค่าเน็ตห้อง | Room internet bill | 1,068.93 | Fixed expense |
| จ่ายค่าบัตรเครดิตกรุงศรี | Krungsri credit card payment | 2,614.00 | Fixed expense |
| จ่ายค่าบัตรกดเงินสดกสิกร | Kasikorn cash card payment | 7,111.92 | Fixed expense |
| จ่ายค่าบัตรเครดิตกสิกร | Kasikorn credit card payment | 3,285.48 | Fixed expense |
| เงินกิน | Food budget (monthly) | 7,000.00 | Budget-vs-Actual |
| **Total expenses** | | **28,530.33** | |

This exact dataset becomes the seed data (see [DATABASE_DESIGN.md](DATABASE_DESIGN.md) §7).

Additional concepts the user wants tracked, mapped to the data model:

| Thai term | English | Model concept |
|---|---|---|
| รายได้จากเงินเดือน | Salary income | `Categories` (Type=Income) + `IncomeEntries` |
| รายได้อื่น | Other income | `Categories` (Type=Income) + `IncomeEntries` |
| ค่าใช้จ่ายคงที่ | Fixed expenses | `ExpenseEntries.Classification = 'Fixed'` |
| ค่าใช้จ่ายไม่คงที่ | Variable expenses | `ExpenseEntries.Classification = 'Variable'` |
| เงินเก็บ | Savings | `SavingGoals` + `SavingContributions` |
| เงินกิน | Food budget | `Categories` (TrackingMode=BudgetVsActual) + `BudgetAllocations` + `Transactions` |
| เงินรางวัลให้ตัวเอง | Self-reward money | Same pattern as food, category "Reward" |
| เงินเหลือ | Remaining money | Computed KPI (see §5) |
| รายการอื่นที่ user สร้างเองได้ | User-defined custom items | Any dynamically created `Category` |

## 4. Functional Requirements

### 4.1 Categories (dynamic, never hard-coded)
- User can create/edit/deactivate categories of `Type ∈ {Income, Expense, Saving}`.
- Expense categories declare `Classification ∈ {Fixed, Variable}` and `TrackingMode ∈ {Simple, BudgetVsActual}`.
- Deactivating a category hides it from "add new" pickers but preserves historical entries (soft delete via `IsActive`).
- A starter set of categories is seeded per new user (Salary, Other Income, Rent, Utilities, Credit Card, Food, Reward, Transportation, Shopping, Entertainment, General Savings) — all editable/deletable except none are protected; this is just a UX convenience.

### 4.2 Monthly Periods
- Every financial fact belongs to exactly one `MonthlyPeriod` (`Year` + `Month`, unique per user).
- User can navigate `< Aug 2026 | Sep 2026 | Oct 2026 >`.
- Creating a new month offers two explicit actions (see UI_UX_SPEC.md §4.2):
  - **Start Empty** — blank month.
  - **Copy from Recurring Templates** — pulls in all active `RecurringTemplates` (income, fixed expenses, budget allocations, saving contributions), recalculating due dates for the new month.
- A month cannot be created twice for the same user/year/month (unique constraint + 409 Conflict).

### 4.3 Income
Each income entry supports: name, amount, category (source), date, note, `IsRecurring`, `IsActive`. Users add/edit/remove/deactivate income sources freely. Recurring income entries can originate from a `RecurringTemplate`.

### 4.4 Expenses — Simple mode
For fixed/variable bills: name, amount, category, `Classification`, `DueDate`, `IsPaid`, `PaidDate`, note, `SortOrder`. Supports recurring templates (e.g., internet bill every month ≈ same amount). Paid/unpaid toggle drives the "Paid vs Unpaid" dashboard widget.

### 4.5 Budget-vs-Actual categories (Food, Reward, Shopping, Transportation, Entertainment, custom)
- `BudgetAllocations`: one planned amount per category per month (e.g., Food = 7,000).
- `Transactions`: individual actual spends logged any time during the month (e.g., grocery run = 620).
- UI always shows **Budget / Used / Remaining** for these categories.
- Quick-add flow optimized for mobile (see UI_UX_SPEC.md §6) — e.g., logging a 620 THB grocery trip should take under 10 seconds on a phone.
- **Rule: a Budget-vs-Actual category never also gets a `ExpenseEntries` row for the same spend.** Money is either tracked as a Simple expense OR as Budget+Transactions, never both, to avoid double counting.

### 4.6 Savings
- `SavingGoals`: name, optional `TargetAmount`, optional `TargetDate`, optional `PlannedMonthlyContribution`.
- `SavingContributions`: actual monthly contributions (amount, date, note), optionally recurring.
- Accumulated amount = `SUM(SavingContributions.Amount)` for a goal across all time (computed, not stored, to avoid drift).
- No investment portfolio tracking, no return/interest projections — explicitly out of scope.

### 4.7 Recurring Items
- A `RecurringTemplate` can generate: an Income entry, a Simple Expense entry, a Budget Allocation, or a Saving Contribution.
- Templates have `Amount` (default, editable per generated entry), `DueDay` (1–31, clipped to month length), `IsActive`.
- "Create Next Month" action copies all active templates belonging to the user into the new `MonthlyPeriod`.

### 4.8 Dashboard (per selected month)
Minimum widgets: Income, Expenses, Savings, Remaining, Food Remaining, Reward Remaining, Paid/Unpaid expense count, monthly progress (day X of N, % elapsed).
Charts (max 4, all responsive):
1. Income vs Expenses vs Savings — last 6 months (bar/area)
2. Expense breakdown by category — current month (donut)
3. Remaining money trend — last 6 months (line)
4. Fixed vs Variable expense comparison — current month (bar)

### 4.9 Monthly History & Comparison
List/grid of past months with totals; clicking a month opens it read/editable (past months stay editable — there is no "locking", per priority #1 simplicity).

### 4.10 Multi-user & Data Isolation
- Every table has `UserId` (directly or via `MonthlyPeriodId`/`CategoryId`/`SavingGoalId` chain).
- The authenticated user's id is resolved server-side from the JWT — **never** trusted from the request body/query string.
- All repository queries filter by the resolved `UserId`. Integration tests must assert cross-user access returns 404 (see TASKS.md Phase 11).

## 5. Accounting Rules (authoritative — full detail in DATABASE_DESIGN.md §6)

For a given `MonthlyPeriod`:

```
TotalIncome     = SUM(IncomeEntries.Amount)
TotalExpenses   = SUM(ExpenseEntries.Amount)                         -- Simple expenses
                + SUM(Transactions.Amount WHERE Category.Type='Expense')  -- actual spend in budget categories
TotalSavings    = SUM(SavingContributions.Amount)
RemainingMoney  = TotalIncome - TotalExpenses - TotalSavings
```

**`BudgetAllocations.AllocatedAmount` is planning metadata only and is NEVER added to `TotalExpenses`.** It is used exclusively to render per-category "Remaining" widgets:

```
CategoryRemaining = BudgetAllocations.AllocatedAmount - SUM(Transactions.Amount for that category/month)
```

Worked example matching the required test case:
- Income = 40,000 → `TotalIncome = 40,000`
- Expenses = 20,000 (across ExpenseEntries + budget-category Transactions combined) → `TotalExpenses = 20,000`
- Savings = 5,000 → `TotalSavings = 5,000`
- `RemainingMoney = 40,000 - 20,000 - 5,000 = 15,000`

## 6. Non-Functional Requirements

| Area | Requirement |
|---|---|
| Performance | Dashboard loads in < 1s on broadband with < 24 months of data; API p95 < 300ms for standard reads |
| Responsiveness | Fully usable at 360px width and up; no horizontal scroll on core screens |
| Security | Hashed passwords (BCrypt), JWT auth, parameterized SQL only, per-request authorization, secrets via env vars |
| Maintainability | One backend project (no microservices), explicit SQL via Dapper, one frontend SPA, clear folder structure |
| Currency | All money stored as `DECIMAL(18,2)`, formatted as Thai Baht (฿12,345.67) in UI |
| Dates | Stored as Gregorian `DATE`/`DATETIME2`; Buddhist Era conversion happens only in the UI presentation layer |
| Localization | UI copy in English by default; Thai labels used for seed category names where natural; no full i18n framework required for v1 |

## 7. Explicitly Out of Scope (v1)

- OAuth / social login / SSO / multi-factor auth
- Multi-currency support
- Investment portfolio performance tracking (returns, holdings, market prices)
- Bank account integration / statement import
- Bill splitting / multi-user shared households
- Notifications/reminders (email/push) — may be a future phase
- Native mobile apps (responsive web only)

## 8. Glossary

| Thai | English |
|---|---|
| ค่าห้อง | Rent |
| เงินกิน | Food money/budget |
| เงินเก็บ | Savings |
| เงินเหลือ | Remaining money |
| เงินรางวัลให้ตัวเอง | Self-reward money |
| รายได้ | Income |
| ค่าใช้จ่าย | Expense |
| คงที่ | Fixed |
| ไม่คงที่ | Variable |

## 9. Related Documents

- [ARCHITECTURE.md](ARCHITECTURE.md) — stack decisions and system design
- [DATABASE_DESIGN.md](DATABASE_DESIGN.md) — full SQL Server schema
- [API_SPEC.md](API_SPEC.md) — REST contract
- [UI_UX_SPEC.md](UI_UX_SPEC.md) — screens, navigation, visual design
- [TASKS.md](TASKS.md) — implementation phases/checklist
- [README.md](README.md) — setup and run instructions
