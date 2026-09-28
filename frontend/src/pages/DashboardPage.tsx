import { Button, Progress, Skeleton, Tag } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { Bar, BarChart, CartesianGrid, Cell, Legend, Line, LineChart, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { monthsApi, reportsApi } from "../api/resources";
import { BudgetProgressCard } from "../components/BudgetProgressCard";
import { EmptyState } from "../components/EmptyState";
import { StatCard } from "../components/StatCard";
import { useCurrentMonth } from "../hooks/useCurrentMonth";
import { formatMonthLabel, formatShortDate, formatShortMonthLabel } from "../lib/buddhist-era";
import { translateCategoryName } from "../lib/categoryLabels";
import { chartMoney } from "../lib/money";
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

  const food = data.categoryBudgets.find((c) => c.name === "Food");
  const reward = data.categoryBudgets.find((c) => c.name === "Reward");
  const barData = [{ name: formatShortMonthLabel(data.year, data.month), [th.chart.income]: data.totalIncome, [th.chart.expenses]: data.totalExpenses, [th.chart.savings]: data.totalSavings }];
  const pieData = (breakdown.data ?? []).map((x) => ({ name: translateCategoryName(x.name), value: x.amount }));
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
        {food && <BudgetProgressCard name={translateCategoryName(food.name)} allocated={food.allocated} used={food.used} remainingDays={data.monthProgress.remainingDays} />}
        {reward && <BudgetProgressCard name={translateCategoryName(reward.name)} allocated={reward.allocated} used={reward.used} remainingDays={data.monthProgress.remainingDays} />}
        <div className="mf-card p-4">
          <div className="text-sm font-medium text-[var(--mf-text-secondary)]">{th.dashboard.unpaidWidget}</div>
          <div className="mt-2 flex items-center gap-2">
            <Tag color="success" className="m-0">{th.dashboard.paidCount(data.paidExpenseCount)}</Tag>
            <Tag color="warning" className="m-0">{th.dashboard.unpaidCount(data.unpaidExpenseCount)}</Tag>
          </div>
          <Button type="link" className="mt-1 px-0" onClick={() => nav("/expenses")}>{th.dashboard.viewUnpaid}</Button>
        </div>
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
        <ChartCard title={th.dashboard.chartBreakdown}>
          <PieChart>
            <Pie data={pieData} dataKey="value" nameKey="name" outerRadius={90} label>
              {pieData.map((_, i) => <Cell key={i} fill={chartColors.sequence[i % chartColors.sequence.length]} />)}
            </Pie>
            <Tooltip formatter={chartMoney} />
            <Legend />
          </PieChart>
        </ChartCard>
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
