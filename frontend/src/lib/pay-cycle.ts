export const DEFAULT_PAYDAY_DAY = 26;

export function paydayOnOrBefore(year: number, month: number, paydayDay: number, skipWeekend: boolean) {
  const last = new Date(year, month, 0).getDate();
  const day = Math.min(Math.max(paydayDay, 1), last);
  const date = new Date(year, month - 1, day);
  if (!skipWeekend) return date;
  const dow = date.getDay();
  if (dow === 6) date.setDate(date.getDate() - 1);
  else if (dow === 0) date.setDate(date.getDate() - 2);
  return date;
}

export function periodContaining(today: Date, paydayDay: number, skipWeekend: boolean) {
  const date = new Date(today.getFullYear(), today.getMonth(), today.getDate());
  const thisStart = paydayOnOrBefore(date.getFullYear(), date.getMonth() + 1, paydayDay, skipWeekend);
  if (date >= thisStart) return { year: date.getFullYear(), month: date.getMonth() + 1 };
  const prev = new Date(date.getFullYear(), date.getMonth() - 1, 1);
  return { year: prev.getFullYear(), month: prev.getMonth() + 1 };
}
