import { Button, DatePicker, Drawer, Form, Input, Select, Switch, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import dayjs from "dayjs";
import { categoriesApi, incomesApi } from "../api/resources";
import type { Income } from "../api/types";
import { confirmDelete } from "../components/ConfirmDeleteModal";
import { MoneyInput } from "../components/MoneyInput";
import { EmptyState } from "../components/EmptyState";
import { PageHeader } from "../components/PageHeader";
import { ResponsiveTable } from "../components/ResponsiveTable";
import { useCurrentMonth } from "../hooks/useCurrentMonth";
import { formatShortDate } from "../lib/buddhist-era";
import { toDateOnly } from "../lib/dates";
import { translateCategoryName } from "../lib/categoryLabels";
import { formatMoney } from "../lib/money";
import { th } from "../locales/th";

export function IncomePage() {
  const { data: month } = useCurrentMonth();
  const id = month?.monthlyPeriodId;
  const qc = useQueryClient();
  const list = useQuery({ queryKey: ["incomes", id], queryFn: () => incomesApi.list(id!), enabled: !!id });
  const cats = useQuery({ queryKey: ["categories", "Income"], queryFn: () => categoriesApi.list("Income") });
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Income | null>(null);
  const [form] = Form.useForm();
  const save = useMutation({
    mutationFn: (v: any) => editing ? incomesApi.update(editing.incomeEntryId, v) : incomesApi.create({ ...v, monthlyPeriodId: id }),
    onSuccess: () => { qc.invalidateQueries(); setOpen(false); setEditing(null); message.success(th.common.saved); }
  });
  const openCreate = () => { setEditing(null); form.resetFields(); setOpen(true); };

  if (!id) return <EmptyState description={th.empty.createMonthFirst} />;

  return (
    <div>
      <PageHeader title={th.income.title} extra={<Button type="primary" onClick={openCreate}>{th.income.addBtn}</Button>} />
      {(list.data ?? []).length === 0 ? (
        <EmptyState description={th.income.empty} cta={th.income.addBtn} onClick={openCreate} />
      ) : (
        <ResponsiveTable
          rowKey="incomeEntryId"
          data={list.data ?? []}
          columns={[
            { title: th.income.columns.name, dataIndex: "name" },
            { title: th.income.columns.category, dataIndex: "categoryName", render: (v: string) => translateCategoryName(v) },
            { title: th.income.columns.amount, dataIndex: "amount", align: "right", render: (v: number) => <span className="tabular">{formatMoney(v)}</span> },
            { title: th.income.columns.date, dataIndex: "incomeDate", render: (v: string) => formatShortDate(v) },
            { title: th.income.columns.active, dataIndex: "isActive", render: (v: boolean, r: Income) => <Switch checked={v} onChange={(c) => incomesApi.setActive(r.incomeEntryId, c).then(() => qc.invalidateQueries())} /> },
            { title: "", render: (_: unknown, r: Income) => <><Button type="link" onClick={() => { setEditing(r); form.setFieldsValue({ ...r, incomeDate: dayjs(r.incomeDate) }); setOpen(true); }}>{th.common.edit}</Button><Button type="link" danger onClick={() => confirmDelete(r.name, () => incomesApi.remove(r.incomeEntryId).then(() => qc.invalidateQueries()))}>{th.common.delete}</Button></> }
          ]}
        />
      )}
      <Drawer title={editing ? th.income.edit : th.income.add} open={open} onClose={() => setOpen(false)} width={420}>
        <Form form={form} layout="vertical" onFinish={(v) => save.mutate({ ...v, incomeDate: toDateOnly(v.incomeDate) })}>
          <Form.Item name="name" label={th.common.name} rules={[{ required: true }]}><Input size="large" /></Form.Item>
          <Form.Item name="categoryId" label={th.common.category} rules={[{ required: true }]}>
            <Select size="large" options={(cats.data ?? []).map((c) => ({ value: c.categoryId, label: translateCategoryName(c.name) }))} showSearch optionFilterProp="label" />
          </Form.Item>
          <Form.Item name="amount" label={th.common.amount} rules={[{ required: true }]}><MoneyInput /></Form.Item>
          <Form.Item name="incomeDate" label={th.common.date} rules={[{ required: true }]}><DatePicker className="w-full" size="large" /></Form.Item>
          <Form.Item name="isRecurring" label={th.income.isRecurring} valuePropName="checked"><Switch /></Form.Item>
          <Form.Item name="note" label={th.common.noteOptional}><Input.TextArea rows={2} /></Form.Item>
          <Button type="primary" htmlType="submit" loading={save.isPending} block size="large">{th.common.save}</Button>
        </Form>
      </Drawer>
    </div>
  );
}
