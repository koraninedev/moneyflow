export function dailyAllowance(remaining: number, todayUsed: number, remainingDays?: number): number | null {
  if (remainingDays == null || remainingDays <= 0) return null;
  const leftover = remaining + todayUsed;
  return leftover > 0 ? leftover / remainingDays : 0;
}

export function isOverDaily(todayUsed: number, allowance: number | null): boolean {
  return allowance != null && todayUsed > allowance;
}
