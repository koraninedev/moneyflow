# API_SPEC.md — MoneyFlow REST API

Base URL (dev): `http://localhost:5000/api`. All endpoints except `/auth/login` and `/auth/register` require `Authorization: Bearer <jwt>`. `UserId` is always resolved from the JWT server-side; it never appears in the request URL/body.

## 1. Conventions

### 1.1 Response Envelope
Success:
```json
{ "data": { }, "error": null }
```
List success:
```json
{ "data": [ ], "meta": { "count": 12 }, "error": null }
```
Error:
```json
{ "data": null, "error": { "code": "VALIDATION_ERROR", "message": "Amount must be greater than 0", "fields": { "amount": "Must be greater than 0" } } }
```

### 1.2 Status Codes
| Code | Meaning |
|---|---|
| 200 | OK (read/update) |
| 201 | Created |
| 204 | No Content (delete) |
| 400 | Validation error |
| 401 | Missing/invalid JWT |
| 403 | Authenticated but not permitted (reserved; not expected in v1 single-tenant-per-user model) |
| 404 | Not found OR belongs to another user (never leak existence) |
| 409 | Conflict (duplicate month, duplicate allocation, category in use) |
| 500 | Unhandled server error |

### 1.3 Common Query Params
- `year`, `month` — select a `MonthlyPeriod` (e.g., `?year=2026&month=9`). Most resource endpoints are scoped to a month via these params or via `monthlyPeriodId`.

### 1.4 Money & Dates
- Amounts are JSON numbers with up to 2 decimals, serialized from `DECIMAL(18,2)`.
- Dates are ISO-8601 `YYYY-MM-DD` (Gregorian). BE conversion is a frontend-only display concern.

## 2. Auth

### `POST /auth/register`
Request: `{ "email": string, "password": string (min 8), "displayName": string }`
Response `201`: `{ "data": { "userId": number, "email": string, "displayName": string } }`
Side effect: seeds default categories for the new user (see DATABASE_DESIGN.md §7/§8 categories, zero balances).
Errors: `409` if email exists.

### `POST /auth/login`
Request: `{ "email": string, "password": string }`
Response `200`: `{ "data": { "token": string, "expiresAt": "2026-10-02T00:00:00Z", "user": { "userId": number, "email": string, "displayName": string } } }`
Errors: `401` invalid credentials.

### `GET /auth/me`
Response `200`: `{ "data": { "userId": number, "email": string, "displayName": string } }`

## 3. Months

### `GET /months`
Returns all monthly periods for the user, newest first, with lightweight totals for the history list.
Response `200`: `{ "data": [ { "monthlyPeriodId": 12, "year": 2026, "month": 9, "totalIncome": 40000, "totalExpenses": 23860.33, "totalSavings": 5000, "remaining": 11139.67 } ] }`

### `GET /months/current`
Returns (creating if missing, empty) the period matching today's server date, plus its full summary (equivalent to `GET /months/{id}/summary`).

### `GET /months/{year}/{month}`
Fetch a specific period + summary. `404` if it doesn't exist yet (frontend then offers "Create this month").

### `POST /months`
Create a new month.
Request: `{ "year": 2026, "month": 10, "mode": "Empty" | "CopyRecurring" }`
Behavior:
- `Empty`: creates a bare `MonthlyPeriod`.
- `CopyRecurring`: creates the period, then for every active `RecurringTemplate` owned by the user, inserts the corresponding `IncomeEntry` / `ExpenseEntry` / `BudgetAllocation` / `SavingContribution` row with `DueDay` resolved against the new month's actual day count, `IsPaid = false` where applicable.
Response `201`: created period + summary.
Errors: `409` if `(year, month)` already exists for this user.

### `GET /months/{monthlyPeriodId}/summary`
The full dashboard payload for one month.
Response `200`:
```json
{
  "data": {
    "monthlyPeriodId": 12, "year": 2026, "month": 9,
    "totalIncome": 40000.00,
    "totalExpenses": 23860.33,
    "fixedExpenses": 14330.33,
    "variableExpenses": 9530.00,
    "totalBudgetAllocation": 9000.00,
    "totalBudgetActualUsed": 2330.00,
    "totalSavings": 5000.00,
    "remaining": 11139.67,
    "paidExpenseCount": 4, "unpaidExpenseCount": 2,
    "monthProgress": { "dayOfMonth": 25, "daysInMonth": 30, "percentElapsed": 83.3 },
    "categoryBudgets": [
      { "categoryId": 7, "name": "Food", "allocated": 7000.00, "used": 2330.00, "remaining": 4670.00 },
      { "categoryId": 8, "name": "Reward", "allocated": 2000.00, "used": 0.00, "remaining": 2000.00 }
    ]
  }
}
```

## 4. Categories

### `GET /categories?type=Income|Expense|Saving`
List active categories for the user, optionally filtered by type.

### `POST /categories`
Request: `{ "name": string, "type": "Income"|"Expense"|"Saving", "classification": "Fixed"|"Variable"|null, "trackingMode": "Simple"|"BudgetVsActual", "icon": string|null, "color": string|null }`
Response `201`: created category.

### `PUT /categories/{categoryId}`
Update name/classification/trackingMode/icon/color/sortOrder. `404` if not owned by caller.

### `DELETE /categories/{categoryId}`
Soft-deletes (`IsActive = 0`). Returns `409` only if the frontend explicitly requests hard delete and rows reference it (v1 always soft-deletes, so this effectively always succeeds with `204`).

## 5. Income

### `GET /incomes?monthlyPeriodId={id}`
List income entries for a month.

### `POST /incomes`
Request: `{ "monthlyPeriodId": number, "categoryId": number, "name": string, "amount": number, "incomeDate": "YYYY-MM-DD", "isRecurring": boolean, "note": string|null }`
Response `201`.

### `PUT /incomes/{incomeEntryId}`
Full update of an income entry.

### `PATCH /incomes/{incomeEntryId}/active`
Request: `{ "isActive": boolean }` — toggle without full edit.

### `DELETE /incomes/{incomeEntryId}`
Hard delete (income entries are not referenced elsewhere). `204`.

## 6. Expenses (Simple mode)

### `GET /expenses?monthlyPeriodId={id}`
List simple expense entries for a month, ordered by `SortOrder`.

### `POST /expenses`
Request: `{ "monthlyPeriodId": number, "categoryId": number, "name": string, "amount": number, "classification": "Fixed"|"Variable", "dueDate": "YYYY-MM-DD"|null, "note": string|null }`
Validation: `categoryId` must reference a category with `Type='Expense'` and `TrackingMode='Simple'` → else `400`.
Response `201`.

### `PUT /expenses/{expenseEntryId}`
Full update.

### `PATCH /expenses/{expenseEntryId}/paid`
Request: `{ "isPaid": boolean, "paidDate": "YYYY-MM-DD"|null }` — quick toggle used by the dashboard/table row action.

### `DELETE /expenses/{expenseEntryId}`
`204`.

### `PUT /expenses/reorder`
Request: `{ "monthlyPeriodId": number, "orderedIds": [number, ...] }` — bulk update `SortOrder`.

## 7. Budget Allocations (Budget-vs-Actual planning side)

### `GET /budgets?monthlyPeriodId={id}`
List allocations for a month, each including computed `used`/`remaining` (same shape as `categoryBudgets` in the month summary).

### `POST /budgets`
Request: `{ "monthlyPeriodId": number, "categoryId": number, "allocatedAmount": number, "note": string|null }`
Validation: category must have `TrackingMode='BudgetVsActual'` and `Type='Expense'`. `409` if an allocation already exists for this `(monthlyPeriodId, categoryId)` — use `PUT` instead.

### `PUT /budgets/{budgetAllocationId}`
Update `allocatedAmount`/`note`.

### `DELETE /budgets/{budgetAllocationId}`
`204`.

## 8. Transactions (Budget-vs-Actual actual spend)

### `GET /transactions?monthlyPeriodId={id}&categoryId={optional}`
List actual transactions, optionally filtered by category, newest first.

### `POST /transactions`
Request: `{ "monthlyPeriodId": number, "categoryId": number, "amount": number, "transactionDate": "YYYY-MM-DD", "description": string, "note": string|null }`
This is the endpoint used by the mobile **quick-add** grocery/reward flow. Validation: category must have `TrackingMode='BudgetVsActual'`.
Response `201`.

### `POST /transactions/quick-add`
Convenience endpoint pre-populating `transactionDate = today` server-side; body only needs `{ "categoryId": number, "amount": number, "description": string }`. Resolves the current month automatically. Optimized for the mobile FAB flow described in UI_UX_SPEC.md §6.

### `PUT /transactions/{transactionId}`
Full update.

### `DELETE /transactions/{transactionId}`
`204`.

## 9. Savings

### `GET /savings/goals`
List the user's saving goals with computed `accumulatedAmount` (`SUM(SavingContributions.Amount)` across all months).

### `POST /savings/goals`
Request: `{ "name": string, "targetAmount": number|null, "targetDate": "YYYY-MM-DD"|null, "plannedMonthlyContribution": number|null }`

### `PUT /savings/goals/{savingGoalId}`
Update goal fields.

### `DELETE /savings/goals/{savingGoalId}`
Soft delete (`IsActive=0`); blocked with `409` only if called as a hard delete while contributions exist (v1 always soft-deletes).

### `GET /savings/contributions?monthlyPeriodId={id}`
List contributions for a month (optionally `&savingGoalId=`).

### `POST /savings/contributions`
Request: `{ "monthlyPeriodId": number, "savingGoalId": number, "amount": number, "contributionDate": "YYYY-MM-DD", "note": string|null }`

### `DELETE /savings/contributions/{savingContributionId}`
`204`.

## 10. Recurring Templates

### `GET /recurring-items`
List all templates for the user (all target types).

### `POST /recurring-items`
Request: `{ "targetType": "Income"|"ExpenseEntry"|"BudgetAllocation"|"SavingContribution", "categoryId": number|null, "savingGoalId": number|null, "name": string, "amount": number, "classification": "Fixed"|"Variable"|null, "dueDay": number|null }`

### `PUT /recurring-items/{recurringTemplateId}`
Update template fields.

### `PATCH /recurring-items/{recurringTemplateId}/active`
Toggle `IsActive` without deleting history/links.

### `DELETE /recurring-items/{recurringTemplateId}`
`204`. Any entries generated from it keep their data; the API sets `RecurringTemplateId` to `NULL` on them, then deletes the template (`ON DELETE NO ACTION` at the database — SQL Server cannot use `SET NULL` here).

## 11. Dashboard

### `GET /dashboard?monthlyPeriodId={id}`
Same payload as `GET /months/{id}/summary` (kept as an alias for a clearer frontend route name — implemented by calling the same Service method, not duplicated logic).

## 12. Reports

### `GET /reports/trend?months=6`
Last N months (including current), oldest first.
Response: `{ "data": [ { "year": 2026, "month": 4, "totalIncome": ..., "totalExpenses": ..., "totalSavings": ..., "remaining": ... }, ... ] }`
Powers dashboard chart 1 (Income vs Expenses vs Savings) and chart 3 (Remaining trend).

### `GET /reports/category-breakdown?monthlyPeriodId={id}`
Response: `{ "data": [ { "categoryId": 3, "name": "Rent", "type": "Expense", "amount": 7200.00 }, ... ] }`
Combines `ExpenseEntries` + budget-category `Transactions` grouped by category — powers dashboard chart 2 (expense breakdown donut).

### `GET /reports/fixed-vs-variable?monthlyPeriodId={id}`
Response: `{ "data": { "fixed": 14330.33, "variable": 9530.00 } }` — powers dashboard chart 4.

## 13. Endpoint Summary Table

| Method | Path | Purpose |
|---|---|---|
| POST | /auth/register | Create account |
| POST | /auth/login | Get JWT |
| GET | /auth/me | Current user info |
| GET | /months | Month history list |
| GET | /months/current | Current month + summary |
| GET | /months/{year}/{month} | Specific month + summary |
| POST | /months | Create month (Empty/CopyRecurring) |
| GET | /months/{id}/summary | Full dashboard payload |
| GET/POST/PUT/DELETE | /categories[/{id}] | Category CRUD |
| GET/POST/PUT/PATCH/DELETE | /incomes[/{id}] | Income CRUD |
| GET/POST/PUT/PATCH/DELETE | /expenses[/{id}] | Simple expense CRUD + paid toggle |
| PUT | /expenses/reorder | Bulk sort order |
| GET/POST/PUT/DELETE | /budgets[/{id}] | Budget allocation CRUD |
| GET/POST/PUT/DELETE | /transactions[/{id}] | Actual transaction CRUD |
| POST | /transactions/quick-add | Fast mobile add |
| GET/POST/PUT/DELETE | /savings/goals[/{id}] | Saving goal CRUD |
| GET/POST/DELETE | /savings/contributions[/{id}] | Contribution CRUD |
| GET/POST/PUT/PATCH/DELETE | /recurring-items[/{id}] | Recurring template CRUD |
| GET | /dashboard | Dashboard summary (alias) |
| GET | /reports/trend | Multi-month trend |
| GET | /reports/category-breakdown | Category donut data |
| GET | /reports/fixed-vs-variable | Fixed vs variable bar data |
