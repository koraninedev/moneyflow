import { Card, Progress } from "antd";
import { dailyAllowance, isOverDaily } from "../lib/budgetPace";
import { formatMoney } from "../lib/money";
import { th } from "../locales/th";

export function BudgetProgressCard({ name, allocated, used, todayUsed = 0, remainingDays, pace = "day", onClick }: { name: string; allocated: number; used: number; todayUsed?: number; remainingDays?: number; pace?: "day" | "week"; onClick?: () => void }) {
  const pct = allocated > 0 ? (used / allocated) * 100 : 0;
  const remaining = allocated - used;
  const over = pct > 100;
  const status = over ? "exception" : pct >= 80 ? "normal" : "success";
  const stroke = over ? "var(--mf-danger)" : pct >= 80 ? "var(--mf-warning)" : "var(--mf-success)";
  let paceLabel: string | null = null;
  if (remainingDays != null && remainingDays > 0 && remaining > 0) {
    const amount = remaining / remainingDays * (pace === "week" ? 7 : 1);
    paceLabel = pace === "week" ? th.budgets.weeklyPace(formatMoney(amount)) : th.budgets.dailyPace(formatMoney(amount));
  }
  const allowance = dailyAllowance(remaining, todayUsed, remainingDays);
  const overDaily = isOverDaily(todayUsed, allowance);
  return (
    <Card hoverable={!!onClick} className="mf-card h-full cursor-pointer transition-shadow hover:shadow-md" styles={{ body: { padding: 16 } }} onClick={onClick}>
      <div className="mb-2 flex items-center justify-between gap-3">
        <span className="font-medium text-[var(--mf-text)]">{name}</span>
        <span className="tabular text-sm text-[var(--mf-text-secondary)]">{formatMoney(used)} / {formatMoney(allocated)}</span>
      </div>
      <Progress percent={Math.min(pct, 100)} status={status} strokeColor={stroke} showInfo={false} size="small" />
      <div className={`mt-2 flex items-center justify-between gap-3 text-sm ${over ? "text-[var(--mf-danger)]" : "text-[var(--mf-text)]"}`}>
        <span className="font-medium">{over ? `${th.status.overBudget} ${formatMoney(used - allocated)}` : `${th.common.remaining} ${formatMoney(remaining)}`}</span>
        {paceLabel != null && <span className="shrink-0 text-xs font-medium text-[var(--mf-text-secondary)]">{paceLabel}</span>}
      </div>
      {allowance != null && todayUsed > 0 && (
        <div className={`mt-1.5 text-xs font-medium ${overDaily ? "text-[var(--mf-danger)]" : "text-[var(--mf-text-secondary)]"}`}>
          {overDaily ? th.budgets.todayOver(formatMoney(todayUsed), formatMoney(allowance)) : th.budgets.todayUsed(formatMoney(todayUsed), formatMoney(allowance))}
        </div>
      )}
    </Card>
  );
}
