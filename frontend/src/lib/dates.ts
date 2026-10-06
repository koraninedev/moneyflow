import dayjs, { type Dayjs } from "dayjs";

export function toDateOnly(value?: Dayjs | Date | string | null): string | undefined {
  if (value == null || value === "") return undefined;
  const d = dayjs(value);
  return d.isValid() ? d.format("YYYY-MM-DD") : undefined;
}

export function todayDateOnly(): string {
  return dayjs().format("YYYY-MM-DD");
}

export function dateOnly(value?: Dayjs | Date | string | null): string {
  if (value == null || value === "") return "";
  if (typeof value === "string") return value.slice(0, 10);
  return dayjs(value).isValid() ? dayjs(value).format("YYYY-MM-DD") : "";
}

export function maxEntryDate(periodEnd?: string | null): string {
  const today = todayDateOnly();
  const end = dateOnly(periodEnd);
  return end && end < today ? end : today;
}

export function minEntryDate(periodStart?: string | null): string {
  return dateOnly(periodStart) || todayDateOnly();
}

export function clampEntryDate(date: string, periodStart?: string | null, periodEnd?: string | null): string {
  const min = minEntryDate(periodStart);
  const max = maxEntryDate(periodEnd);
  if (date < min) return min;
  if (date > max) return max;
  return date;
}
