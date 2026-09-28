import { Button, Drawer, Form, Input, Select, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { categoriesApi, txnsApi } from "../api/resources";
import { MoneyInput } from "./MoneyInput";
import { translateCategoryName } from "../lib/categoryLabels";
import { th } from "../locales/th";

export function QuickAddSheet({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [form] = Form.useForm();
  const qc = useQueryClient();
  const cats = useQuery({ queryKey: ["categories", "Expense"], queryFn: () => categoriesApi.list("Expense") });
  const budgetCats = (cats.data ?? []).filter((c) => c.trackingMode === "BudgetVsActual");
  const mut = useMutation({
    mutationFn: (v: { categoryId: number; amount: number; description: string }) => txnsApi.quickAdd(v),
    onSuccess: async (txn) => {
      await qc.invalidateQueries();
      onClose();
      form.resetFields();
      message.open({
        type: "success",
        content: (
          <span>
            {th.quickAdd.addedTo(translateCategoryName(txn.categoryName))}{" "}
            <Button type="link" size="small" onClick={() => txnsApi.remove(txn.transactionId).then(() => qc.invalidateQueries())}>{th.common.undo}</Button>
          </span>
        )
      });
    }
  });
  return (
    <Drawer title={th.quickAdd.title} placement="bottom" height={380} open={open} onClose={onClose} styles={{ body: { paddingTop: 8 } }}>
      <Form form={form} layout="vertical" onFinish={(v) => mut.mutate(v)}>
        <Form.Item name="categoryId" label={th.quickAdd.category} rules={[{ required: true }]}>
          <Select size="large" options={budgetCats.map((c) => ({ value: c.categoryId, label: translateCategoryName(c.name) }))} showSearch optionFilterProp="label" />
        </Form.Item>
        <Form.Item name="amount" label={th.quickAdd.amount} rules={[{ required: true }]}>
          <MoneyInput />
        </Form.Item>
        <Form.Item name="description" label={th.quickAdd.description} rules={[{ required: true }]}>
          <Input size="large" placeholder={th.quickAdd.descriptionPlaceholder} />
        </Form.Item>
        <Button type="primary" htmlType="submit" loading={mut.isPending} block size="large">{th.quickAdd.submit}</Button>
      </Form>
    </Drawer>
  );
}
