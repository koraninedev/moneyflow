import { RightOutlined } from "@ant-design/icons";
import { Button, Card, Progress, Skeleton, Tag } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Bar, BarChart, CartesianGrid, Cell, Legend, Line, LineChart, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { monthsApi, reportsApi } from "../api/resources";
import type { CategoryBudget } from "../api/types";
import { BudgetProgressCard } from "../components/BudgetProgressCard";
import { BudgetTxnDrawer } from "../components/BudgetTxnDrawer";
import { EmptyState } from "../components/EmptyState";
import { StatCard } from "../components/StatCard";
import { useCurrentMonth } from "../hooks/useCurrentMonth";
import { formatMonthLabel, formatShortDate, formatShortMonthLabel } from "../lib/buddhist-era";
import { translateCategoryName } from "../lib/categoryLabels";
import { chartMoney, formatMoney } from "../lib/money";
import { th } from "../locales/th";
import { chartColors } from "../theme/tokens";

export function DashboardPage() {
  const { year, month, data, isLoading, isError, refetch } = useCurrentMonth();
  const qc = useQueryClient();
  const nav = useNavigate();
  const create = useMutation({
    mutationFn: (mode: "Empty" | "CopyRecurring") => monthsApi.create(year, month, mode),
    onSuccess: () => qc.invalidateQueries()
  });
  const missing = isError && !data;
  const id = data?.monthlyPeriodId;
  const [activeBudget, setActiveBudget] = useState<CategoryBudget | null>(null);
  const trend = useQuery({ queryKey: ["trend", 6], queryFn: () => reportsApi.trend(6), enabled: !!id });
  const breakdown = useQuery({ queryKey: ["breakdown", id], queryFn: () => reportsApi.breakdown(id!), enabled: !!id });
  const fv = useQuery({ queryKey: ["fv", id], queryFn: () => reportsApi.fixedVsVariable(id!), enabled: !!id });

  if (isLoading) return <Skeleton active paragraph={{ rows: 8 }} />;
  if (missing) {
    return (
      <div className="py-10">
        <EmptyState description={th.dashboard.noMonth(formatMonthLabel(year, month))} />
        <div className="mt-4 flex justify-center gap-3">
          <Button type="primary" loading={create.isPending} onClick={() => create.mutate("Empty")}>{th.dashboard.startEmpty}</Button>
          <Button loading={create.isPending} onClick={() => create.mutate("CopyRecurring")}>{th.dashboard.copyRecurring}</Button>
        </div>
      </div>
    );
  }
  if (!data) return <Button onClick={() => refetch()}>{th.common.retry}</Button>;

  const barData = [{ name: formatShortMonthLabel(data.year, data.month), [th.chart.income]: data.totalIncome, [th.chart.expenses]: data.totalExpenses, [th.chart.savings]: data.totalSavings }];
  const pieData = (breakdown.data ?? []).filter((x) => x.amount > 0).map((x) => ({ name: translateCategoryName(x.name), value: x.amount }));
  const lineData = (trend.data ?? []).map((t) => ({ name: formatShortMonthLabel(t.year, t.month), [th.chart.remaining]: t.remaining }));
  const fvData = [
    { name: th.chart.fixed, amount: fv.data?.fixed ?? data.fixedExpenses },
    { name: th.chart.variable, amount: fv.data?.variable ?? data.variableExpenses }
  ];

  return (
    <div className="flex flex-col gap-5">
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-4">
        <div className="lg:col-span-1">
          <StatCard label={th.dashboard.remaining} value={data.remaining} emphasis />
        </div>
        <div className="grid grid-cols-3 gap-4 lg:col-span-3">
          <StatCard label={th.dashboard.income} value={data.totalIncome} tone="pos" />
          <StatCard label={th.dashboard.expenses} value={data.totalExpenses} tone="neg" />
          <StatCard label={th.dashboard.savings} value={data.totalSavings} tone="primary" />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <Card className="mf-card h-full" styles={{ body: { padding: 16 } }}>
          <div className="mb-2 flex items-center justify-between gap-3">
            <span className="font-medium text-[var(--mf-text)]">{th.dashboard.unpaidWidget}</span>
            <span className="tabular text-sm text-[var(--mf-text-secondary)]">{th.dashboard.unpaidWidgetTotal(data.paidExpenseCount + data.unpaidExpenseCount)}</span>
          </div>
          <p className="m-0 truncate text-xs leading-[22px] text-[var(--mf-text-secondary)]">{th.dashboard.unpaidWidgetHint}</p>
          <div className="mt-2 flex items-center justify-between gap-3">
            <div className="flex min-w-0 flex-wrap gap-2">
              <Tag color="success" className="m-0">{th.dashboard.paidCount(data.paidExpenseCount)}</Tag>
              <Tag color="warning" className="m-0">{th.dashboard.unpaidCount(data.unpaidExpenseCount)}</Tag>
            </div>
            <Button type="text" size="small" className="shrink-0 !h-auto !px-1.5 font-medium text-[var(--mf-primary)]" onClick={() => nav("/expenses")}>{th.dashboard.viewUnpaid}<RightOutlined className="ml-1 text-[10px]" /></Button>
          </div>
        </Card>
        {data.categoryBudgets.map((b) => (
          <BudgetProgressCard key={b.categoryId} name={translateCategoryName(b.name)} allocated={b.allocated} used={b.used} remainingDays={data.monthProgress.remainingDays} onClick={() => setActiveBudget(b)} />
        ))}
      </div>

      <div className="mf-card p-4">
        <div className="mb-1 text-sm font-medium text-[var(--mf-text-secondary)]">
          {th.dashboard.monthProgress(data.monthProgress.dayOfMonth, data.monthProgress.daysInMonth, data.monthProgress.percentElapsed)}
          <span className="ml-2 font-normal">({formatShortDate(data.monthProgress.periodStart)} – {formatShortDate(data.monthProgress.periodEnd)})</span>
        </div>
        <Progress percent={data.monthProgress.percentElapsed} showInfo={false} strokeColor={chartColors.savings} />
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <ChartCard title={th.dashboard.chartIncomeExpenseSavings}>
          <BarChart data={barData}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--mf-border)" />
            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
            <YAxis tick={{ fontSize: 12 }} />
            <Tooltip formatter={chartMoney} />
            <Legend />
            <Bar dataKey={th.chart.income} fill={chartColors.income} radius={[4, 4, 0, 0]} />
            <Bar dataKey={th.chart.expenses} fill={chartColors.expenses} radius={[4, 4, 0, 0]} />
            <Bar dataKey={th.chart.savings} fill={chartColors.savings} radius={[4, 4, 0, 0]} />
          </BarChart>
        </ChartCard>
        <PieBreakdownCard title={th.dashboard.chartBreakdown} data={pieData} />
        <ChartCard title={th.dashboard.chartTrend}>
          <LineChart data={lineData}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--mf-border)" />
            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
            <YAxis tick={{ fontSize: 12 }} />
            <Tooltip formatter={chartMoney} />
            <Line type="monotone" dataKey={th.chart.remaining} stroke={chartColors.remaining} strokeWidth={2} dot={false} />
          </LineChart>
        </ChartCard>
        <ChartCard title={th.dashboard.chartFixedVariable}>
          <BarChart data={fvData}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--mf-border)" />
            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
            <YAxis tick={{ fontSize: 12 }} />
            <Tooltip formatter={chartMoney} />
            <Bar dataKey="amount" radius={[4, 4, 0, 0]}>
              {fvData.map((_, i) => <Cell key={i} fill={i === 0 ? chartColors.fixed : chartColors.variable} />)}
            </Bar>
          </BarChart>
        </ChartCard>
      </div>

      {data.totalIncome === 0 && data.totalExpenses === 0 && (
        <EmptyState description={th.dashboard.emptyAllTitle} cta={`+ ${th.dashboard.emptyAllCta}`} onClick={() => nav("/income")} />
      )}
      <BudgetTxnDrawer open={!!activeBudget} title={activeBudget ? translateCategoryName(activeBudget.name) : ""} monthlyPeriodId={id} categoryId={activeBudget?.categoryId} onClose={() => setActiveBudget(null)} />
    </div>
  );
}

function ChartCard({ title, children }: { title: string; children: React.ReactElement }) {
  return (
    <div className="mf-card h-72 p-4">
      <div className="mb-2 text-sm font-semibold text-[var(--mf-text)]">{title}</div>
      <ResponsiveContainer width="100%" height="88%">{children}</ResponsiveContainer>
    </div>
  );
}

function PieBreakdownCard({ title, data }: { title: string; data: { name: string; value: number }[] }) {
  const total = data.reduce((sum, x) => sum + x.value, 0);
  return (
    <div className="mf-card flex h-72 flex-col p-4">
      <div className="mb-2 text-sm font-semibold text-[var(--mf-text)]">{title}</div>
      {data.length === 0 ? (
        <div className="flex flex-1 items-center justify-center text-sm text-[var(--mf-text-secondary)]">{th.dashboard.chartBreakdownEmpty}</div>
      ) : (
        <div className="flex min-h-0 flex-1">
          <div className="w-[46%]">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={data} dataKey="value" nameKey="name" cx="50%" cy="50%" innerRadius={48} outerRadius={78} paddingAngle={2} stroke="var(--mf-surface)">
                  {data.map((x, i) => <Cell key={x.name} fill={chartColors.sequence[i % chartColors.sequence.length]} />)}
                </Pie>
                <Tooltip formatter={chartMoney} />
              </PieChart>
            </ResponsiveContainer>
          </div>
          <div className="flex w-[54%] flex-col justify-center gap-1.5 overflow-auto pl-1 pr-1">
            {data.map((x, i) => (
              <div key={x.name} className="flex items-center gap-2 text-xs">
                <span className="h-2.5 w-2.5 shrink-0 rounded-sm" style={{ background: chartColors.sequence[i % chartColors.sequence.length] }} />
                <span className="min-w-0 flex-1 truncate text-[var(--mf-text)]">{x.name}</span>
                <span className="shrink-0 text-[var(--mf-text-muted)]">{total > 0 ? `${Math.round((x.value / total) * 100)}%` : ""}</span>
                <span className="w-[5.75rem] shrink-0 tabular text-right text-[var(--mf-text-secondary)]">{formatMoney(x.value)}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
