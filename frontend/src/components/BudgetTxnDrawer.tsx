import { Button, Drawer, Form, Input, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { txnsApi } from "../api/resources";
import type { Txn } from "../api/types";
import { formatShortDate } from "../lib/buddhist-era";
import { todayDateOnly } from "../lib/dates";
import { formatMoney } from "../lib/money";
import { th } from "../locales/th";
import { MoneyInput } from "./MoneyInput";

export function BudgetTxnDrawer({ open, title, monthlyPeriodId, categoryId, onClose }: { open: boolean; title: string; monthlyPeriodId?: number; categoryId?: number; onClose: () => void }) {
  const [form] = Form.useForm();
  const qc = useQueryClient();
  const txns = useQuery({ queryKey: ["txns", monthlyPeriodId, categoryId], queryFn: () => txnsApi.list(monthlyPeriodId!, categoryId), enabled: open && !!monthlyPeriodId && !!categoryId });
  const addTxn = useMutation({
    mutationFn: (v: any) => txnsApi.create({ ...v, monthlyPeriodId, categoryId, transactionDate: todayDateOnly() }),
    onSuccess: () => { form.resetFields(); qc.invalidateQueries(); message.success(th.common.added); }
  });

  return (
    <Drawer title={title} open={open} onClose={() => { form.resetFields(); onClose(); }} width={420}>
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
