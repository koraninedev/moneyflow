import { Card, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useIsMobile } from "../hooks/useIsMobile";
import { translateCategoryName } from "../lib/categoryLabels";
import { formatMoney } from "../lib/money";

export function ResponsiveTable<T extends { [k: string]: any }>({ columns, data, rowKey, onRowClick }: { columns: ColumnsType<T>; data: T[]; rowKey: string; onRowClick?: (row: T) => void }) {
  const isMobile = useIsMobile();
  if (isMobile) {
    return (
      <div className="flex flex-col gap-2.5">
        {data.map((row) => (
          <Card key={String(row[rowKey])} className="mf-card cursor-pointer" styles={{ body: { padding: 14 } }} onClick={() => onRowClick?.(row)}>
            <div className="flex items-center justify-between gap-2">
              <span className="font-medium text-[var(--mf-text)]">{row.name ?? row.description ?? "รายการ"}</span>
              <span className="tabular shrink-0 font-medium text-[var(--mf-text)]">{row.amount != null ? formatMoney(row.amount) : ""}</span>
            </div>
            <div className="mt-1 text-sm text-[var(--mf-text-secondary)]">{row.categoryName ? translateCategoryName(row.categoryName) : (row.classification ?? "")}</div>
          </Card>
        ))}
      </div>
    );
  }
  return (
    <div className="mf-card overflow-hidden">
      <Table rowKey={rowKey} columns={columns} dataSource={data} pagination={false} onRow={(row) => ({ onClick: () => onRowClick?.(row) })} />
    </div>
  );
}
