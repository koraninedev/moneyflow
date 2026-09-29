import { Button, Form, Modal, Segmented, Select, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { budgetsApi, categoriesApi } from "../api/resources";
import type { Budget } from "../api/types";
import { BudgetProgressCard } from "../components/BudgetProgressCard";
import { BudgetTxnDrawer } from "../components/BudgetTxnDrawer";
import { MoneyInput } from "../components/MoneyInput";
import { EmptyState } from "../components/EmptyState";
import { PageHeader } from "../components/PageHeader";
import { useCurrentMonth } from "../hooks/useCurrentMonth";
import { translateCategoryName } from "../lib/categoryLabels";
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
  const [pace, setPace] = useState<"day" | "week">("day");
  const addAlloc = useMutation({
    mutationFn: (v: any) => budgetsApi.create({ ...v, monthlyPeriodId: id }),
    onSuccess: () => { qc.invalidateQueries(); setAllocOpen(false); message.success(th.common.saved); }
  });

  if (!id) return <EmptyState description={th.empty.createMonthFirst} />;

  return (
    <div>
      <PageHeader
        title={th.budgets.title}
        extra={
          <>
            {(list.data ?? []).length > 0 && (
              <div className="flex items-center gap-2">
                <span className="hidden text-sm text-[var(--mf-text-secondary)] sm:inline">{th.budgets.paceLabel}</span>
                <Segmented value={pace} onChange={(v) => setPace(v as "day" | "week")} options={[{ label: th.budgets.paceDay, value: "day" }, { label: th.budgets.paceWeek, value: "week" }]} />
              </div>
            )}
            <Button type="primary" onClick={() => setAllocOpen(true)}>{th.budgets.setAllocation}</Button>
          </>
        }
      />
      {(list.data ?? []).length === 0 ? (
        <EmptyState description={th.budgets.empty} cta={th.budgets.setAllocation} onClick={() => setAllocOpen(true)} />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {(list.data ?? []).map((b) => (
            <BudgetProgressCard key={b.budgetAllocationId} name={translateCategoryName(b.categoryName)} allocated={b.allocatedAmount} used={b.used} remainingDays={month?.monthProgress.remainingDays} pace={pace} onClick={() => setActive(b)} />
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
      <BudgetTxnDrawer open={!!active} title={active ? translateCategoryName(active.categoryName) : ""} monthlyPeriodId={id} categoryId={active?.categoryId} onClose={() => setActive(null)} />
    </div>
  );
}
