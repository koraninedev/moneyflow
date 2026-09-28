import { Card, Progress } from "antd";
import { th } from "../locales/th";
import { formatMoney } from "../lib/money";

export function BudgetProgressCard({ name, allocated, used, remainingDays, onClick }: { name: string; allocated: number; used: number; remainingDays?: number; onClick?: () => void }) {
  const pct = allocated > 0 ? (used / allocated) * 100 : 0;
  const remaining = allocated - used;
  const over = pct > 100;
  const status = over ? "exception" : pct >= 80 ? "normal" : "success";
  const stroke = over ? "var(--mf-danger)" : pct >= 80 ? "var(--mf-warning)" : "var(--mf-success)";
  const perDay = remainingDays != null && remainingDays > 0 && remaining > 0 ? remaining / remainingDays : null;
  return (
    <Card hoverable={!!onClick} className="mf-card cursor-pointer transition-shadow hover:shadow-md" onClick={onClick}>
      <div className="mb-2 flex items-center justify-between">
        <span className="font-medium text-[var(--mf-text)]">{name}</span>
        <span className="tabular text-sm text-[var(--mf-text-secondary)]">{formatMoney(used)} / {formatMoney(allocated)}</span>
      </div>
      <Progress percent={Math.min(pct, 100)} status={status} strokeColor={stroke} showInfo={false} size="small" />
      <div className={`mt-2 flex items-baseline justify-between gap-3 text-sm ${over ? "text-[var(--mf-danger)]" : "text-[var(--mf-text)]"}`}>
        <span className="font-medium">{over ? `${th.status.overBudget} ${formatMoney(used - allocated)}` : `${th.common.remaining} ${formatMoney(remaining)}`}</span>
        {perDay != null && <span className="shrink-0 text-xs font-medium text-[var(--mf-text-secondary)]">{th.budgets.dailyPace(formatMoney(perDay))}</span>}
      </div>
    </Card>
  );
}
