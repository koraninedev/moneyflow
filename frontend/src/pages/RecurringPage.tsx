import { Button, Form, Input, InputNumber, Modal, Select, Switch, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { categoriesApi, recurringApi, savingsApi } from "../api/resources";
import { MoneyInput } from "../components/MoneyInput";
import { PageHeader } from "../components/PageHeader";
import { translateCategoryName } from "../lib/categoryLabels";
import { formatMoney } from "../lib/money";
import { th } from "../locales/th";

const GROUP_ORDER = ["Income", "ExpenseEntry", "BudgetAllocation", "SavingContribution"] as const;

export function RecurringPage() {
  const qc = useQueryClient();
  const list = useQuery({ queryKey: ["recurring"], queryFn: recurringApi.list });
  const cats = useQuery({ queryKey: ["categories"], queryFn: () => categoriesApi.list() });
  const goals = useQuery({ queryKey: ["goals"], queryFn: savingsApi.goals });
  const [open, setOpen] = useState(false);
  const create = useMutation({ mutationFn: recurringApi.create, onSuccess: () => { qc.invalidateQueries(); setOpen(false); message.success(th.common.created); } });

  return (
    <div>
      <PageHeader title={th.recurring.title} subtitle={th.recurring.subtitle} extra={<Button type="primary" onClick={() => setOpen(true)}>{th.recurring.add}</Button>} />
      <div className="flex flex-col gap-4">
        {GROUP_ORDER.map((g) => {
          const rows = (list.data ?? []).filter((r) => r.targetType === g);
          if (rows.length === 0) return null;
          return (
            <div key={g} className="mf-card p-4">
              <h2 className="mb-2 text-sm font-semibold text-[var(--mf-text-secondary)]">{th.recurring.groups[g]}</h2>
              <div className="flex flex-col">
                {rows.map((r) => (
                  <div key={r.recurringTemplateId} className="flex items-center justify-between border-b border-[var(--mf-border)] py-2.5 last:border-0">
                    <span className="text-[var(--mf-text)]">
                      {r.name} · <span className="tabular">{formatMoney(r.amount)}</span>
                      {r.dueDay ? ` · วันที่ ${r.dueDay}` : ""}
                    </span>
                    <Switch checked={r.isActive} onChange={(c) => recurringApi.setActive(r.recurringTemplateId, c).then(() => qc.invalidateQueries())} />
                  </div>
                ))}
              </div>
            </div>
          );
        })}
      </div>
      <Modal title={th.recurring.addTitle} open={open} onCancel={() => setOpen(false)} footer={null}>
        <Form layout="vertical" onFinish={(v) => create.mutate(v)} initialValues={{ targetType: "ExpenseEntry" }}>
          <Form.Item name="targetType" label={th.recurring.type}>
            <Select size="large" options={GROUP_ORDER.map((g) => ({ value: g, label: th.recurring.groups[g] }))} />
          </Form.Item>
          <Form.Item name="name" label={th.common.name} rules={[{ required: true }]}><Input size="large" /></Form.Item>
          <Form.Item name="amount" label={th.common.amount} rules={[{ required: true }]}><MoneyInput /></Form.Item>
          <Form.Item name="categoryId" label={th.common.category}>
            <Select allowClear size="large" options={(cats.data ?? []).map((c) => ({ value: c.categoryId, label: translateCategoryName(c.name) }))} />
          </Form.Item>
          <Form.Item name="savingGoalId" label={th.recurring.savingGoal}>
            <Select allowClear size="large" options={(goals.data ?? []).map((g) => ({ value: g.savingGoalId, label: g.name }))} />
          </Form.Item>
          <Form.Item name="classification" label={th.categories.classification}>
            <Select allowClear size="large" options={[{ value: "Fixed", label: th.categories.classifications.fixed }, { value: "Variable", label: th.categories.classifications.variable }]} />
          </Form.Item>
          <Form.Item name="dueDay" label={th.recurring.dueDay}><InputNumber className="w-full" size="large" min={1} max={31} /></Form.Item>
          <Button type="primary" htmlType="submit" loading={create.isPending} block size="large">{th.common.save}</Button>
        </Form>
      </Modal>
    </div>
  );
}
