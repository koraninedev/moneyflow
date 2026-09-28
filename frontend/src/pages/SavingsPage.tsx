import { Button, Card, DatePicker, Form, Input, Modal, Progress, message } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { savingsApi } from "../api/resources";
import { EmptyState } from "../components/EmptyState";
import { MoneyInput } from "../components/MoneyInput";
import { PageHeader } from "../components/PageHeader";
import { useCurrentMonth } from "../hooks/useCurrentMonth";
import { toDateOnly, todayDateOnly } from "../lib/dates";
import { formatMoney } from "../lib/money";
import { th } from "../locales/th";

export function SavingsPage() {
  const { data: month } = useCurrentMonth();
  const id = month?.monthlyPeriodId;
  const qc = useQueryClient();
  const goals = useQuery({ queryKey: ["goals"], queryFn: savingsApi.goals });
  const [goalOpen, setGoalOpen] = useState(false);
  const [contribGoal, setContribGoal] = useState<number | null>(null);
  const createGoal = useMutation({ mutationFn: savingsApi.createGoal, onSuccess: () => { qc.invalidateQueries(); setGoalOpen(false); message.success(th.common.created); } });
  const contrib = useMutation({
    mutationFn: (v: any) => savingsApi.createContribution({ ...v, monthlyPeriodId: id, savingGoalId: contribGoal, contributionDate: todayDateOnly() }),
    onSuccess: () => { qc.invalidateQueries(); setContribGoal(null); message.success(th.common.added); }
  });

  const list = goals.data ?? [];

  return (
    <div>
      <PageHeader title={th.savings.title} subtitle={th.savings.subtitle} extra={<Button type="primary" onClick={() => setGoalOpen(true)}>{th.savings.newGoal}</Button>} />
      {list.length === 0 ? (
        <EmptyState description={th.savings.emptyTitle} cta={th.savings.newGoal} onClick={() => setGoalOpen(true)} />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {list.map((g) => {
            const hasTarget = !!g.targetAmount;
            const pct = hasTarget ? Math.min(100, Math.round((g.accumulatedAmount / g.targetAmount!) * 1000) / 10) : 0;
            return (
              <Card key={g.savingGoalId} className="mf-card">
                <div className="mb-2 font-semibold text-[var(--mf-text)]">{g.name}</div>
                <div className="tabular text-2xl font-semibold text-[var(--mf-primary)]">{formatMoney(g.accumulatedAmount)}</div>
                {hasTarget && <div className="mt-0.5 text-sm text-[var(--mf-text-secondary)]">{th.savings.ofTarget(formatMoney(g.targetAmount!))}</div>}
                {hasTarget && (
                  <div className="mt-3">
                    <div className="mb-1 text-xs font-medium text-[var(--mf-text-secondary)]">{pct}%</div>
                    <Progress percent={pct} strokeColor="var(--mf-primary)" showInfo={false} />
                  </div>
                )}
                {g.plannedMonthlyContribution ? (
                  <div className="mt-3 text-sm text-[var(--mf-text-secondary)]">{th.savings.planLabel(formatMoney(g.plannedMonthlyContribution))}</div>
                ) : null}
                <Button className="mt-4" block onClick={() => setContribGoal(g.savingGoalId)}>{th.savings.contribute}</Button>
              </Card>
            );
          })}
        </div>
      )}

      <Modal title={th.savings.newGoalTitle} open={goalOpen} onCancel={() => setGoalOpen(false)} footer={null}>
        <Form layout="vertical" onFinish={(v) => createGoal.mutate({ ...v, targetDate: toDateOnly(v.targetDate) })}>
          <Form.Item name="name" label={th.savings.goalName} rules={[{ required: true }]}><Input size="large" /></Form.Item>
          <Form.Item name="targetAmount" label={th.savings.targetAmount}><MoneyInput min={0} /></Form.Item>
          <Form.Item name="targetDate" label={th.savings.targetDate}><DatePicker className="w-full" size="large" /></Form.Item>
          <Form.Item name="plannedMonthlyContribution" label={th.savings.plannedMonthly}><MoneyInput min={0} /></Form.Item>
          <Button type="primary" htmlType="submit" loading={createGoal.isPending} block size="large">{th.common.save}</Button>
        </Form>
      </Modal>

      <Modal title={th.savings.contributeTitle} open={contribGoal != null} onCancel={() => setContribGoal(null)} footer={null}>
        <Form layout="vertical" onFinish={(v) => contrib.mutate(v)}>
          <Form.Item name="amount" label={th.common.amount} rules={[{ required: true }]}><MoneyInput /></Form.Item>
          <Form.Item name="note" label={th.common.noteOptional}><Input size="large" /></Form.Item>
          <Button type="primary" htmlType="submit" loading={contrib.isPending} block size="large" disabled={!id}>{th.common.save}</Button>
        </Form>
      </Modal>
    </div>
  );
}
