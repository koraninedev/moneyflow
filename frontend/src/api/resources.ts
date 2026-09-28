import { apiDelete, apiGet, apiPatch, apiPost, apiPut } from "./client";
import type { Budget, Category, CategoryBreakdown, Expense, FixedVsVariable, Income, LoginResponse, MonthListItem, MonthSummary, Recurring, SavingContribution, SavingGoal, TrendPoint, Txn, User } from "./types";

export const authApi = {
  login: (email: string, password: string) => apiPost<LoginResponse>("/auth/login", { email, password }),
  register: (email: string, password: string, displayName: string) => apiPost<User>("/auth/register", { email, password, displayName }),
  me: () => apiGet<User>("/auth/me"),
  updateSettings: (body: { periodStartDay: number; skipWeekendPayday: boolean }) => apiPut<User>("/auth/settings", body)
};

export const monthsApi = {
  list: () => apiGet<MonthListItem[]>("/months"),
  current: () => apiGet<MonthSummary>("/months/current"),
  get: (year: number, month: number) => apiGet<MonthSummary>(`/months/${year}/${month}`),
  create: (year: number, month: number, mode: "Empty" | "CopyRecurring") => apiPost<MonthSummary>("/months", { year, month, mode }),
  summary: (id: number) => apiGet<MonthSummary>(`/months/${id}/summary`)
};

export const dashboardApi = { get: (monthlyPeriodId: number) => apiGet<MonthSummary>(`/dashboard?monthlyPeriodId=${monthlyPeriodId}`) };
export const categoriesApi = {
  list: (type?: string) => apiGet<Category[]>(`/categories${type ? `?type=${type}` : ""}`),
  create: (body: Partial<Category>) => apiPost<Category>("/categories", body),
  update: (id: number, body: Partial<Category>) => apiPut<Category>(`/categories/${id}`, body),
  remove: (id: number) => apiDelete(`/categories/${id}`)
};
export const incomesApi = {
  list: (monthlyPeriodId: number) => apiGet<Income[]>(`/incomes?monthlyPeriodId=${monthlyPeriodId}`),
  create: (body: object) => apiPost<Income>("/incomes", body),
  update: (id: number, body: object) => apiPut<Income>(`/incomes/${id}`, body),
  setActive: (id: number, isActive: boolean) => apiPatch(`/incomes/${id}/active`, { isActive }),
  remove: (id: number) => apiDelete(`/incomes/${id}`)
};
export const expensesApi = {
  list: (monthlyPeriodId: number) => apiGet<Expense[]>(`/expenses?monthlyPeriodId=${monthlyPeriodId}`),
  create: (body: object) => apiPost<Expense>("/expenses", body),
  update: (id: number, body: object) => apiPut<Expense>(`/expenses/${id}`, body),
  setPaid: (id: number, isPaid: boolean, paidDate?: string) => apiPatch(`/expenses/${id}/paid`, { isPaid, paidDate }),
  reorder: (monthlyPeriodId: number, orderedIds: number[]) => apiPut("/expenses/reorder", { monthlyPeriodId, orderedIds }),
  remove: (id: number) => apiDelete(`/expenses/${id}`)
};
export const budgetsApi = {
  list: (monthlyPeriodId: number) => apiGet<Budget[]>(`/budgets?monthlyPeriodId=${monthlyPeriodId}`),
  create: (body: object) => apiPost<Budget>("/budgets", body),
  update: (id: number, body: object) => apiPut<Budget>(`/budgets/${id}`, body),
  remove: (id: number) => apiDelete(`/budgets/${id}`)
};
export const txnsApi = {
  list: (monthlyPeriodId: number, categoryId?: number) => apiGet<Txn[]>(`/transactions?monthlyPeriodId=${monthlyPeriodId}${categoryId ? `&categoryId=${categoryId}` : ""}`),
  create: (body: object) => apiPost<Txn>("/transactions", body),
  quickAdd: (body: object) => apiPost<Txn>("/transactions/quick-add", body),
  update: (id: number, body: object) => apiPut<Txn>(`/transactions/${id}`, body),
  remove: (id: number) => apiDelete(`/transactions/${id}`)
};
export const savingsApi = {
  goals: () => apiGet<SavingGoal[]>("/savings/goals"),
  createGoal: (body: object) => apiPost<SavingGoal>("/savings/goals", body),
  updateGoal: (id: number, body: object) => apiPut<SavingGoal>(`/savings/goals/${id}`, body),
  removeGoal: (id: number) => apiDelete(`/savings/goals/${id}`),
  contributions: (monthlyPeriodId: number, savingGoalId?: number) => apiGet<SavingContribution[]>(`/savings/contributions?monthlyPeriodId=${monthlyPeriodId}${savingGoalId ? `&savingGoalId=${savingGoalId}` : ""}`),
  createContribution: (body: object) => apiPost<SavingContribution>("/savings/contributions", body),
  removeContribution: (id: number) => apiDelete(`/savings/contributions/${id}`)
};
export const recurringApi = {
  list: () => apiGet<Recurring[]>("/recurring-items"),
  create: (body: object) => apiPost<Recurring>("/recurring-items", body),
  update: (id: number, body: object) => apiPut<Recurring>(`/recurring-items/${id}`, body),
  setActive: (id: number, isActive: boolean) => apiPatch(`/recurring-items/${id}/active`, { isActive }),
  remove: (id: number) => apiDelete(`/recurring-items/${id}`)
};
export const reportsApi = {
  trend: (months = 6) => apiGet<TrendPoint[]>(`/reports/trend?months=${months}`),
  breakdown: (monthlyPeriodId: number) => apiGet<CategoryBreakdown[]>(`/reports/category-breakdown?monthlyPeriodId=${monthlyPeriodId}`),
  fixedVsVariable: (monthlyPeriodId: number) => apiGet<FixedVsVariable>(`/reports/fixed-vs-variable?monthlyPeriodId=${monthlyPeriodId}`)
};
