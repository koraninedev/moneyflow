import { create } from "zustand";
import { DEFAULT_PAYDAY_DAY, periodContaining } from "../lib/pay-cycle";

const initial = periodContaining(new Date(), DEFAULT_PAYDAY_DAY, true);
type MonthState = { year: number; month: number; setMonth: (year: number, month: number) => void; shift: (delta: number) => void; syncFromPayday: (periodStartDay: number, skipWeekendPayday: boolean) => void };

export const useMonthStore = create<MonthState>((set, get) => ({
  year: initial.year,
  month: initial.month,
  setMonth: (year, month) => set({ year, month }),
  shift: (delta) => {
    const d = new Date(get().year, get().month - 1 + delta, 1);
    set({ year: d.getFullYear(), month: d.getMonth() + 1 });
  },
  syncFromPayday: (periodStartDay, skipWeekendPayday) => {
    if (!periodStartDay) return;
    const next = periodContaining(new Date(), periodStartDay, skipWeekendPayday);
    set({ year: next.year, month: next.month });
  }
}));
