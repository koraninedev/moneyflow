import { Button, Drawer, Form, Input, Progress, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { txnsApi } from "../api/resources";
import type { Txn } from "../api/types";
import { formatShortDate } from "../lib/buddhist-era";
import { dailyAllowance, isOverDaily } from "../lib/budgetPace";
import { todayDateOnly } from "../lib/dates";
import { formatMoney } from "../lib/money";
import { th } from "../locales/th";
import { MoneyInput } from "./MoneyInput";

export function BudgetTxnDrawer({ open, title, monthlyPeriodId, categoryId, allocated = 0, used = 0, remainingDays, onClose }: { open: boolean; title: string; monthlyPeriodId?: number; categoryId?: number; allocated?: number; used?: number; remainingDays?: number; onClose: () => void }) {
  const [form] = Form.useForm();
  const qc = useQueryClient();
  const amountWatch = Form.useWatch("amount", form);
  const today = todayDateOnly();
  const txns = useQuery({ queryKey: ["txns", monthlyPeriodId, categoryId], queryFn: () => txnsApi.list(monthlyPeriodId!, categoryId), enabled: open && !!monthlyPeriodId && !!categoryId });
  const todayUsed = (txns.data ?? []).reduce((sum, t: Txn) => sum + (String(t.transactionDate).slice(0, 10) === today ? Number(t.amount) : 0), 0);
  const remaining = allocated - used;
  const allowance = dailyAllowance(remaining, todayUsed, remainingDays);
  const projected = todayUsed + Number(amountWatch || 0);
  const overDaily = isOverDaily(projected, allowance);
  const addTxn = useMutation({
    mutationFn: (v: any) => txnsApi.create({ ...v, monthlyPeriodId, categoryId, transactionDate: todayDateOnly() }),
    onSuccess: (_, v) => {
      form.resetFields();
      qc.invalidateQueries();
      const next = todayUsed + Number(v.amount || 0);
      if (isOverDaily(next, allowance) && allowance != null) message.warning(th.budgets.overDailyToast(formatMoney(next), formatMoney(allowance)));
      else message.success(th.common.added);
    }
  });

  return (
    <Drawer title={title} open={open} onClose={() => { form.resetFields(); onClose(); }} width={420}>
      {allowance != null && (
        <div className={`mb-4 rounded-xl px-3 py-2.5 ${overDaily ? "bg-[var(--mf-danger-soft)]" : "bg-[var(--mf-bg)]"}`}>
          <div className="mb-1.5 flex items-center justify-between gap-3 text-xs">
            <span className="font-medium text-[var(--mf-text)]">{th.budgets.todayQuota}</span>
            <span className={`tabular ${overDaily ? "font-medium text-[var(--mf-danger)]" : "text-[var(--mf-text-secondary)]"}`}>{formatMoney(projected)} / {formatMoney(allowance)}</span>
          </div>
          <Progress percent={allowance > 0 ? Math.min((projected / allowance) * 100, 100) : projected > 0 ? 100 : 0} showInfo={false} size="small" status={overDaily ? "exception" : "normal"} strokeColor={overDaily ? "var(--mf-danger)" : "var(--mf-primary)"} />
          {overDaily && <div className="mt-1 text-xs text-[var(--mf-danger)]">{th.budgets.overDailyHint(formatMoney(projected - allowance))}</div>}
        </div>
      )}
      <div className="mb-4 flex flex-col">
        {(txns.data ?? []).map((t: Txn) => (
          <div key={t.transactionId} className="flex items-start justify-between gap-3 border-b border-[var(--mf-border)] py-2.5">
            <div className="min-w-0">
              <div className="truncate text-[var(--mf-text)]">{t.description}</div>
              <div className="mt-0.5 text-xs text-[var(--mf-text-secondary)]">{formatShortDate(t.transactionDate)}</div>
            </div>
            <span className="shrink-0 tabular font-medium">{formatMoney(t.amount)}</span>
          </div>
        ))}
      </div>
      <Form form={form} layout="vertical" onFinish={(v) => addTxn.mutate(v)}>
        <Form.Item name="amount" label={th.common.amount} rules={[{ required: true }]}><MoneyInput /></Form.Item>
        <Form.Item name="description" label={th.quickAdd.description} rules={[{ required: true }]}><Input size="large" /></Form.Item>
        <Button type="primary" htmlType="submit" loading={addTxn.isPending} block size="large">{th.budgets.addTxn}</Button>
      </Form>
    </Drawer>
  );
}
