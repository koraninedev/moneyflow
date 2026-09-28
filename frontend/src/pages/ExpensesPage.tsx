import { Button, DatePicker, Drawer, Form, Input, Segmented, Select, Switch, Tag, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import dayjs from "dayjs";
import { categoriesApi, expensesApi } from "../api/resources";
import type { Expense } from "../api/types";
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

export function ExpensesPage() {
  const { data: month } = useCurrentMonth();
  const id = month?.monthlyPeriodId;
  const qc = useQueryClient();
  const list = useQuery({ queryKey: ["expenses", id], queryFn: () => expensesApi.list(id!), enabled: !!id });
  const cats = useQuery({ queryKey: ["categories", "Expense"], queryFn: () => categoriesApi.list("Expense") });
  const simpleCats = (cats.data ?? []).filter((c) => c.trackingMode === "Simple");
  const [filter, setFilter] = useState(th.expenses.filters.all);
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Expense | null>(null);
  const [form] = Form.useForm();

  const rows = useMemo(() => {
    const all = list.data ?? [];
    if (filter === th.expenses.filters.fixed) return all.filter((e) => e.classification === "Fixed");
    if (filter === th.expenses.filters.variable) return all.filter((e) => e.classification === "Variable");
    if (filter === th.expenses.filters.paid) return all.filter((e) => e.isPaid);
    if (filter === th.expenses.filters.unpaid) return all.filter((e) => !e.isPaid);
    return all;
  }, [list.data, filter]);

  const save = useMutation({
    mutationFn: (v: any) => editing ? expensesApi.update(editing.expenseEntryId, v) : expensesApi.create({ ...v, monthlyPeriodId: id }),
    onSuccess: () => { qc.invalidateQueries(); setOpen(false); setEditing(null); message.success(th.common.saved); }
  });
  const move = async (idx: number, dir: -1 | 1) => {
    const next = [...rows];
    const j = idx + dir;
    if (j < 0 || j >= next.length) return;
    [next[idx], next[j]] = [next[j], next[idx]];
    await expensesApi.reorder(id!, next.map((e) => e.expenseEntryId));
    await qc.invalidateQueries({ queryKey: ["expenses", id] });
  };
  const openCreate = () => { setEditing(null); form.resetFields(); form.setFieldsValue({ classification: "Variable" }); setOpen(true); };

  if (!id) return <EmptyState description={th.empty.createMonthFirst} />;

  return (
    <div>
      <PageHeader title={th.expenses.title} extra={<Button type="primary" onClick={openCreate}>{th.expenses.addBtn}</Button>} />
      <Segmented
        className="mb-4"
        value={filter}
        onChange={(v) => setFilter(String(v))}
        options={[th.expenses.filters.all, th.expenses.filters.fixed, th.expenses.filters.variable, th.expenses.filters.paid, th.expenses.filters.unpaid]}
      />
      {(list.data ?? []).length === 0 ? (
        <EmptyState description={th.expenses.empty} cta={th.expenses.addBtn} onClick={openCreate} />
      ) : (
        <ResponsiveTable
          rowKey="expenseEntryId"
          data={rows}
          columns={[
            { title: th.expenses.columns.name, dataIndex: "name" },
            { title: th.expenses.columns.category, dataIndex: "categoryName", render: (v: string) => translateCategoryName(v) },
            { title: th.expenses.columns.classification, dataIndex: "classification", render: (v: string) => <Tag>{v === "Fixed" ? th.categories.classifications.fixed : th.categories.classifications.variable}</Tag> },
            { title: th.expenses.columns.amount, dataIndex: "amount", align: "right", render: (v: number) => <span className="tabular">{formatMoney(v)}</span> },
            { title: th.expenses.columns.due, dataIndex: "dueDate", render: (v?: string) => formatShortDate(v) },
            { title: th.expenses.columns.paid, dataIndex: "isPaid", render: (v: boolean, r: Expense) => <Switch checked={v} onChange={(c) => expensesApi.setPaid(r.expenseEntryId, c).then(() => qc.invalidateQueries())} /> },
            { title: th.expenses.columns.order, render: (_: unknown, r: Expense, i: number) => <><Button size="small" onClick={() => move(i, -1)}>↑</Button><Button size="small" onClick={() => move(i, 1)}>↓</Button></> },
            { title: "", render: (_: unknown, r: Expense) => <><Button type="link" onClick={() => { setEditing(r); form.setFieldsValue({ ...r, dueDate: r.dueDate ? dayjs(r.dueDate) : undefined }); setOpen(true); }}>{th.common.edit}</Button><Button type="link" danger onClick={() => confirmDelete(r.name, () => expensesApi.remove(r.expenseEntryId).then(() => qc.invalidateQueries()))}>{th.common.delete}</Button></> }
          ]}
        />
      )}
      <Drawer title={editing ? th.expenses.edit : th.expenses.add} open={open} onClose={() => setOpen(false)} width={420}>
        <Form form={form} layout="vertical" onFinish={(v) => save.mutate({ ...v, dueDate: toDateOnly(v.dueDate), sortOrder: editing?.sortOrder ?? 0 })}>
          <Form.Item name="name" label={th.common.name} rules={[{ required: true }]}><Input size="large" /></Form.Item>
          <Form.Item name="categoryId" label={th.common.category} rules={[{ required: true }]}>
            <Select size="large" options={simpleCats.map((c) => ({ value: c.categoryId, label: translateCategoryName(c.name) }))} showSearch optionFilterProp="label" />
          </Form.Item>
          <Form.Item name="amount" label={th.common.amount} rules={[{ required: true }]}><MoneyInput /></Form.Item>
          <Form.Item name="classification" label={th.expenses.classification} rules={[{ required: true }]}>
            <Segmented options={[{ label: th.categories.classifications.fixed, value: "Fixed" }, { label: th.categories.classifications.variable, value: "Variable" }]} />
          </Form.Item>
          <Form.Item name="dueDate" label={th.expenses.dueDate}><DatePicker className="w-full" size="large" /></Form.Item>
          <Form.Item name="note" label={th.common.noteOptional}><Input.TextArea rows={2} /></Form.Item>
          <Button type="primary" htmlType="submit" loading={save.isPending} block size="large">{th.common.save}</Button>
        </Form>
      </Drawer>
    </div>
  );
}
