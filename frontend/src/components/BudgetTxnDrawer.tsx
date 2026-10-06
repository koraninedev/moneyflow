import { DeleteOutlined, EditOutlined } from "@ant-design/icons";
import { Button, DatePicker, Drawer, Form, Input, Progress, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useEffect, useState } from "react";
import { txnsApi } from "../api/resources";
import type { Txn } from "../api/types";
import { useIsMobile } from "../hooks/useIsMobile";
import { formatShortDate } from "../lib/buddhist-era";
import { dailyAllowance, isOverDaily } from "../lib/budgetPace";
import { clampEntryDate, dateOnly, maxEntryDate, minEntryDate, todayDateOnly } from "../lib/dates";
import { formatMoney } from "../lib/money";
import { th } from "../locales/th";
import { confirmDelete } from "./ConfirmDeleteModal";
import { MoneyInput } from "./MoneyInput";

export function BudgetTxnDrawer({ open, title, monthlyPeriodId, categoryId, allocated = 0, used = 0, remainingDays, periodStart, periodEnd, onClose }: { open: boolean; title: string; monthlyPeriodId?: number; categoryId?: number; allocated?: number; used?: number; remainingDays?: number; periodStart?: string; periodEnd?: string; onClose: () => void }) {
  const [form] = Form.useForm();
  const qc = useQueryClient();
  const mobile = useIsMobile();
  const [editing, setEditing] = useState<Txn | null>(null);
  const [selectedDate, setSelectedDate] = useState(todayDateOnly());
  const amountWatch = Form.useWatch("amount", form);
  const minDate = minEntryDate(periodStart);
  const maxDate = maxEntryDate(periodEnd);
  const today = todayDateOnly();
  const txns = useQuery({ queryKey: ["txns", monthlyPeriodId, categoryId], queryFn: () => txnsApi.list(monthlyPeriodId!, categoryId), enabled: open && !!monthlyPeriodId && !!categoryId });
  const dayTxns = (txns.data ?? []).filter((t: Txn) => dateOnly(t.transactionDate) === selectedDate);
  const dayUsed = dayTxns.reduce((sum, t) => sum + Number(t.amount), 0);
  const todayUsed = (txns.data ?? []).reduce((sum, t: Txn) => sum + (dateOnly(t.transactionDate) === today ? Number(t.amount) : 0), 0);
  const editingOnDay = editing != null && dateOnly(editing.transactionDate) === selectedDate;
  const baseline = dayUsed - (editingOnDay ? Number(editing.amount) : 0);
  const typed = amountWatch != null && amountWatch !== "" ? Number(amountWatch) : editingOnDay ? Number(editing.amount) : 0;
  const projected = baseline + (Number.isFinite(typed) ? typed : 0);
  const remaining = allocated - used;
  const allowance = dailyAllowance(remaining, todayUsed, remainingDays);
  const overDaily = isOverDaily(projected, allowance);

  useEffect(() => {
    if (!open) { form.resetFields(); setEditing(null); return; }
    setSelectedDate(clampEntryDate(todayDateOnly(), periodStart, periodEnd));
    form.resetFields();
    setEditing(null);
  }, [open, periodStart, periodEnd, form]);

  const afterSave = (next: number, added: boolean) => {
    form.resetFields();
    setEditing(null);
    qc.invalidateQueries();
    if (isOverDaily(next, allowance) && allowance != null) message.warning(th.budgets.overDailyToast(formatMoney(next), formatMoney(allowance)));
    else message.success(added ? th.common.added : th.common.saved);
  };
  const addTxn = useMutation({
    mutationFn: (v: { amount: number; description: string }) => txnsApi.create({ ...v, monthlyPeriodId, categoryId, transactionDate: selectedDate }),
    onSuccess: (_, v) => afterSave(baseline + Number(v.amount || 0), true)
  });
  const updateTxn = useMutation({
    mutationFn: (v: { amount: number; description: string }) => txnsApi.update(editing!.transactionId, { categoryId, amount: v.amount, description: v.description, transactionDate: selectedDate, note: editing?.note ?? null }),
    onSuccess: (_, v) => afterSave(baseline + Number(v.amount || 0), false)
  });
  const removeTxn = useMutation({
    mutationFn: (id: number) => txnsApi.remove(id),
    onSuccess: (_void, id) => {
      if (editing?.transactionId === id) { form.resetFields(); setEditing(null); }
      qc.invalidateQueries();
      message.success(th.common.deleted);
    }
  });

  const startEdit = (t: Txn) => {
    setEditing(t);
    setSelectedDate(clampEntryDate(dateOnly(t.transactionDate), periodStart, periodEnd));
    form.setFieldsValue({ amount: t.amount, description: t.description });
  };
  const cancelEdit = () => { setEditing(null); form.resetFields(); };
  const changeDate = (d: dayjs.Dayjs | null) => {
    if (!d) return;
    setSelectedDate(clampEntryDate(d.format("YYYY-MM-DD"), periodStart, periodEnd));
    setEditing(null);
    form.resetFields();
  };
  const close = () => { form.resetFields(); setEditing(null); onClose(); };
  const saving = addTxn.isPending || updateTxn.isPending;

  return (
    <Drawer title={title} open={open} onClose={close} width={mobile ? "100%" : 420} styles={{ body: { display: "flex", flexDirection: "column", padding: 16, overflow: "hidden" } }}>
      <div className="shrink-0">
        <div className="mb-1 text-sm font-medium text-[var(--mf-text)]">{th.budgets.entryDate}</div>
        <DatePicker className="w-full" size="large" allowClear={false} inputReadOnly value={dayjs(selectedDate)} minDate={dayjs(minDate)} maxDate={dayjs(maxDate)} disabledDate={(d) => { const v = d.format("YYYY-MM-DD"); return v < minDate || v > maxDate; }} onChange={changeDate} />
        <p className="mt-1 mb-3 text-xs text-[var(--mf-text-secondary)]">{th.budgets.entryDateHint}</p>
        {allowance != null && (
          <div className={`mb-3 rounded-xl px-3 py-2.5 ${overDaily ? "bg-[var(--mf-danger-soft)]" : "bg-[var(--mf-bg)]"}`}>
            <div className="mb-1.5 flex items-center justify-between gap-3 text-xs">
              <span className="font-medium text-[var(--mf-text)]">{th.budgets.dayQuota} · {formatShortDate(selectedDate)}</span>
              <span className={`tabular ${overDaily ? "font-medium text-[var(--mf-danger)]" : "text-[var(--mf-text-secondary)]"}`}>{formatMoney(projected)} / {formatMoney(allowance)}</span>
            </div>
            <Progress percent={allowance > 0 ? Math.min((projected / allowance) * 100, 100) : projected > 0 ? 100 : 0} showInfo={false} size="small" status={overDaily ? "exception" : "normal"} strokeColor={overDaily ? "var(--mf-danger)" : "var(--mf-primary)"} />
            {overDaily && <div className="mt-1 text-xs text-[var(--mf-danger)]">{th.budgets.overDailyHint(formatMoney(projected - allowance))}</div>}
          </div>
        )}
      </div>
      <div className="min-h-0 flex-1 overflow-y-auto">
        {dayTxns.length === 0 ? (
          <div className="py-6 text-center text-sm text-[var(--mf-text-secondary)]">{th.budgets.noTxnsOnDate}</div>
        ) : dayTxns.map((t: Txn) => (
          <div key={t.transactionId} className={`flex items-center gap-2 border-b border-[var(--mf-border)] py-2.5 ${editing?.transactionId === t.transactionId ? "bg-[var(--mf-bg)]" : ""}`}>
            <div className="min-w-0 flex-1">
              <div className="truncate text-[var(--mf-text)]">{t.description}</div>
              <div className="mt-0.5 text-xs text-[var(--mf-text-secondary)]">{formatShortDate(t.transactionDate)}</div>
            </div>
            <span className="shrink-0 tabular font-medium">{formatMoney(t.amount)}</span>
            <div className="flex shrink-0">
              <Button type="text" aria-label={th.common.edit} icon={<EditOutlined />} onClick={() => startEdit(t)} />
              <Button type="text" danger aria-label={th.common.delete} icon={<DeleteOutlined />} onClick={() => confirmDelete(t.description, () => removeTxn.mutate(t.transactionId))} />
            </div>
          </div>
        ))}
      </div>
      <Form className="mt-3 shrink-0 border-t border-[var(--mf-border)] pt-3" form={form} layout="vertical" onFinish={(v) => editing ? updateTxn.mutate(v) : addTxn.mutate(v)}>
        <Form.Item name="amount" label={th.common.amount} rules={[{ required: true }]}><MoneyInput /></Form.Item>
        <Form.Item name="description" label={th.quickAdd.description} rules={[{ required: true }]}><Input size="large" /></Form.Item>
        <div className="flex flex-col gap-2 sm:flex-row">
          {editing && <Button className="sm:flex-1" size="large" onClick={cancelEdit}>{th.common.cancel}</Button>}
          <Button type="primary" htmlType="submit" loading={saving} block size="large" className="sm:flex-[2]">{editing ? th.budgets.saveEdit : th.budgets.addTxn}</Button>
        </div>
      </Form>
    </Drawer>
  );
}
