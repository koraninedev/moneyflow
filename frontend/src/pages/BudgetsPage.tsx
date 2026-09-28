import { Button, Drawer, Form, Input, Modal, Select, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { budgetsApi, categoriesApi, txnsApi } from "../api/resources";
import type { Budget, Txn } from "../api/types";
import { BudgetProgressCard } from "../components/BudgetProgressCard";
import { MoneyInput } from "../components/MoneyInput";
import { EmptyState } from "../components/EmptyState";
import { PageHeader } from "../components/PageHeader";
import { useCurrentMonth } from "../hooks/useCurrentMonth";
import { translateCategoryName } from "../lib/categoryLabels";
import { todayDateOnly } from "../lib/dates";
import { formatMoney } from "../lib/money";
import { th } from "../locales/th";

export function BudgetsPage() {
  const { data: month } = useCurrentMonth();
  const id = month?.monthlyPeriodId;
  const qc = useQueryClient();
  const list = useQuery({ queryKey: ["budgets", id], queryFn: () => budgetsApi.list(id!), enabled: !!id });
  const cats = useQuery({ queryKey: ["categories", "Expense"], queryFn: () => categoriesApi.list("Expense") });
  const budgetCats = (cats.data ?? []).filter((c) => c.trackingMode === "BudgetVsActual");
  const [allocOpen, setAllocOpen] = useState(false);
  const [active, setActive] = useState<Budget | null>(null);
  const txns = useQuery({ queryKey: ["txns", id, active?.categoryId], queryFn: () => txnsApi.list(id!, active!.categoryId), enabled: !!id && !!active });
  const addAlloc = useMutation({
    mutationFn: (v: any) => budgetsApi.create({ ...v, monthlyPeriodId: id }),
    onSuccess: () => { qc.invalidateQueries(); setAllocOpen(false); message.success(th.common.saved); }
  });
  const addTxn = useMutation({
    mutationFn: (v: any) => txnsApi.create({ ...v, monthlyPeriodId: id, categoryId: active?.categoryId, transactionDate: todayDateOnly() }),
    onSuccess: () => { qc.invalidateQueries(); message.success(th.common.added); }
  });

  if (!id) return <EmptyState description={th.empty.createMonthFirst} />;

  return (
    <div>
      <PageHeader title={th.budgets.title} extra={<Button type="primary" onClick={() => setAllocOpen(true)}>{th.budgets.setAllocation}</Button>} />
      {(list.data ?? []).length === 0 ? (
        <EmptyState description={th.budgets.empty} cta={th.budgets.setAllocation} onClick={() => setAllocOpen(true)} />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {(list.data ?? []).map((b) => (
            <BudgetProgressCard key={b.budgetAllocationId} name={translateCategoryName(b.categoryName)} allocated={b.allocatedAmount} used={b.used} remainingDays={month?.monthProgress.remainingDays} onClick={() => setActive(b)} />
          ))}
        </div>
      )}
      <Modal title={th.budgets.setAllocationTitle} open={allocOpen} onCancel={() => setAllocOpen(false)} footer={null}>
        <Form layout="vertical" onFinish={(v) => addAlloc.mutate(v)}>
          <Form.Item name="categoryId" label={th.common.category} rules={[{ required: true }]}>
            <Select size="large" options={budgetCats.map((c) => ({ value: c.categoryId, label: translateCategoryName(c.name) }))} />
          </Form.Item>
          <Form.Item name="allocatedAmount" label={th.budgets.allocated} rules={[{ required: true }]}><MoneyInput min={0} /></Form.Item>
          <Button type="primary" htmlType="submit" loading={addAlloc.isPending} block size="large">{th.common.save}</Button>
        </Form>
      </Modal>
      <Drawer title={active ? translateCategoryName(active.categoryName) : ""} open={!!active} onClose={() => setActive(null)} width={420}>
        <div className="mb-4 flex flex-col gap-2">
          {(txns.data ?? []).map((t: Txn) => (
            <div key={t.transactionId} className="flex justify-between border-b border-[var(--mf-border)] py-2">
              <span className="text-[var(--mf-text)]">{t.description}</span>
              <span className="tabular font-medium">{formatMoney(t.amount)}</span>
            </div>
          ))}
        </div>
        <Form layout="vertical" onFinish={(v) => addTxn.mutate(v)}>
          <Form.Item name="amount" label={th.common.amount} rules={[{ required: true }]}><MoneyInput /></Form.Item>
          <Form.Item name="description" label={th.quickAdd.description} rules={[{ required: true }]}><Input size="large" /></Form.Item>
          <Button type="primary" htmlType="submit" loading={addTxn.isPending} block size="large">{th.budgets.addTxn}</Button>
        </Form>
      </Drawer>
    </div>
  );
}
