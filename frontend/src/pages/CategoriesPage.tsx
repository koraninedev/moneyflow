import { Button, Form, Input, Modal, Radio, Switch, Tabs, Tag, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { categoriesApi } from "../api/resources";
import { confirmDelete } from "../components/ConfirmDeleteModal";
import { PageHeader } from "../components/PageHeader";
import { translateCategoryName } from "../lib/categoryLabels";
import { th } from "../locales/th";

export function CategoriesPage() {
  const qc = useQueryClient();
  const list = useQuery({ queryKey: ["categories"], queryFn: () => categoriesApi.list() });
  const [open, setOpen] = useState(false);
  const [type, setType] = useState("Expense");
  const create = useMutation({ mutationFn: categoriesApi.create, onSuccess: () => { qc.invalidateQueries(); setOpen(false); message.success(th.common.created); } });

  const pane = (t: string) => (list.data ?? []).filter((c) => c.type === t).map((c) => (
    <div key={c.categoryId} className="flex items-center justify-between border-b border-[var(--mf-border)] py-3 last:border-0">
      <div className="flex items-center gap-2">
        <span className="h-2.5 w-2.5 rounded-full" style={{ background: c.color ?? "#2563EB" }} />
        <span className="font-medium text-[var(--mf-text)]">{translateCategoryName(c.name)}</span>
        {c.classification && <Tag>{c.classification === "Fixed" ? th.categories.classifications.fixed : th.categories.classifications.variable}</Tag>}
        <Tag color="blue">{c.trackingMode === "Simple" ? th.categories.trackingModes.simple : th.categories.trackingModes.budget}</Tag>
      </div>
      <div className="flex items-center gap-3">
        <Switch checked={c.isActive} disabled size="small" />
        <Button type="link" danger onClick={() => confirmDelete(translateCategoryName(c.name), () => categoriesApi.remove(c.categoryId).then(() => qc.invalidateQueries()))}>{th.common.delete}</Button>
      </div>
    </div>
  ));

  return (
    <div>
      <PageHeader title={th.categories.title} extra={<Button type="primary" onClick={() => setOpen(true)}>{th.categories.add}</Button>} />
      <div className="mf-card p-4">
        <Tabs
          items={[
            { key: "Income", label: th.categories.types.income, children: pane("Income") },
            { key: "Expense", label: th.categories.types.expense, children: pane("Expense") },
            { key: "Saving", label: th.categories.types.saving, children: pane("Saving") }
          ]}
        />
      </div>
      <Modal title={th.categories.addTitle} open={open} onCancel={() => setOpen(false)} footer={null}>
        <Form layout="vertical" onFinish={(v) => create.mutate({ ...v, trackingMode: v.trackingMode ?? "Simple" })} initialValues={{ type: "Expense", trackingMode: "Simple", classification: "Variable" }} onValuesChange={(v) => v.type && setType(v.type)}>
          <Form.Item name="name" label={th.categories.name} rules={[{ required: true }]}><Input size="large" /></Form.Item>
          <Form.Item name="type" label={th.categories.type}>
            <Radio.Group options={[
              { label: th.categories.types.income, value: "Income" },
              { label: th.categories.types.expense, value: "Expense" },
              { label: th.categories.types.saving, value: "Saving" }
            ]} />
          </Form.Item>
          {type === "Expense" && (
            <>
              <Form.Item name="classification" label={th.categories.classification}>
                <Radio.Group options={[
                  { label: th.categories.classifications.fixed, value: "Fixed" },
                  { label: th.categories.classifications.variable, value: "Variable" }
                ]} />
              </Form.Item>
              <Form.Item name="trackingMode" label={th.categories.tracking}>
                <Radio.Group options={[
                  { label: th.categories.trackingModes.simple, value: "Simple" },
                  { label: th.categories.trackingModes.budget, value: "BudgetVsActual" }
                ]} />
              </Form.Item>
            </>
          )}
          <Button type="primary" htmlType="submit" loading={create.isPending} block size="large">{th.common.save}</Button>
        </Form>
      </Modal>
    </div>
  );
}
