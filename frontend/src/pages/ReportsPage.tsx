import { Card, Segmented, Table } from "antd";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Bar, BarChart, CartesianGrid, Legend, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { reportsApi } from "../api/resources";
import { PageHeader } from "../components/PageHeader";
import { formatShortMonthLabel } from "../lib/buddhist-era";
import { chartMoney, formatMoney } from "../lib/money";
import { th } from "../locales/th";
import { chartColors } from "../theme/tokens";

export function ReportsPage() {
  const [months, setMonths] = useState(6);
  const trend = useQuery({ queryKey: ["trend", months], queryFn: () => reportsApi.trend(months) });
  const rows = trend.data ?? [];
  const chart = rows.map((t) => ({
    name: formatShortMonthLabel(t.year, t.month),
    [th.chart.income]: t.totalIncome,
    [th.chart.expenses]: t.totalExpenses,
    [th.chart.savings]: t.totalSavings,
    [th.chart.remaining]: t.remaining
  }));

  return (
    <div className="flex flex-col gap-4">
      <PageHeader
        title={th.reports.title}
        extra={
          <Segmented
            value={months}
            onChange={(v) => setMonths(Number(v))}
            options={[{ label: th.reports.range.m3, value: 3 }, { label: th.reports.range.m6, value: 6 }, { label: th.reports.range.m12, value: 12 }]}
          />
        }
      />
      <div className="mf-card h-72 p-4">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={chart}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--mf-border)" />
            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
            <YAxis tick={{ fontSize: 12 }} />
            <Tooltip formatter={chartMoney} />
            <Legend />
            <Bar dataKey={th.chart.income} fill={chartColors.income} radius={[4, 4, 0, 0]} />
            <Bar dataKey={th.chart.expenses} fill={chartColors.expenses} radius={[4, 4, 0, 0]} />
            <Bar dataKey={th.chart.savings} fill={chartColors.savings} radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
      <div className="mf-card h-72 p-4">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chart}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--mf-border)" />
            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
            <YAxis tick={{ fontSize: 12 }} />
            <Tooltip formatter={chartMoney} />
            <Line type="monotone" dataKey={th.chart.remaining} stroke={chartColors.remaining} strokeWidth={2} dot={false} />
          </LineChart>
        </ResponsiveContainer>
      </div>
      <div className="mf-card hidden overflow-hidden md:block">
        <Table
          rowKey={(r) => `${r.year}-${r.month}`}
          dataSource={rows}
          pagination={false}
          columns={[
            { title: th.reports.columns.month, render: (_: unknown, r) => formatShortMonthLabel(r.year, r.month) },
            { title: th.reports.columns.income, dataIndex: "totalIncome", align: "right", render: (v: number) => formatMoney(v) },
            { title: th.reports.columns.expenses, dataIndex: "totalExpenses", align: "right", render: (v: number) => formatMoney(v) },
            { title: th.reports.columns.savings, dataIndex: "totalSavings", align: "right", render: (v: number) => formatMoney(v) },
            { title: th.reports.columns.remaining, dataIndex: "remaining", align: "right", render: (v: number) => formatMoney(v) }
          ]}
        />
      </div>
      <div className="flex gap-3 overflow-x-auto md:hidden">
        {rows.map((r) => (
          <Card key={`${r.year}-${r.month}`} className="mf-card min-w-52 shrink-0">
            <div className="mb-1 font-semibold text-[var(--mf-text)]">{formatShortMonthLabel(r.year, r.month)}</div>
            <div className="flex justify-between text-sm"><span className="text-[var(--mf-text-secondary)]">{th.reports.columns.income}</span><span className="tabular">{formatMoney(r.totalIncome)}</span></div>
            <div className="flex justify-between text-sm"><span className="text-[var(--mf-text-secondary)]">{th.reports.columns.expenses}</span><span className="tabular">{formatMoney(r.totalExpenses)}</span></div>
            <div className="flex justify-between text-sm"><span className="text-[var(--mf-text-secondary)]">{th.reports.columns.savings}</span><span className="tabular">{formatMoney(r.totalSavings)}</span></div>
            <div className="flex justify-between text-sm font-medium"><span className="text-[var(--mf-text-secondary)]">{th.reports.columns.remaining}</span><span className="tabular">{formatMoney(r.remaining)}</span></div>
          </Card>
        ))}
      </div>
    </div>
  );
}
