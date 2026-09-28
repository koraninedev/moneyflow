import dayjs, { type Dayjs } from "dayjs";

export function toDateOnly(value?: Dayjs | Date | string | null): string | undefined {
  if (value == null || value === "") return undefined;
  const d = dayjs(value);
  return d.isValid() ? d.format("YYYY-MM-DD") : undefined;
}

export function todayDateOnly(): string {
  return dayjs().format("YYYY-MM-DD");
}
