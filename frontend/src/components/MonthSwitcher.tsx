import { LeftOutlined, RightOutlined } from "@ant-design/icons";
import { Button } from "antd";
import { formatMonthLabel, formatShortMonthLabel } from "../lib/buddhist-era";
import { useMonthStore } from "../stores/month.store";

export function MonthSwitcher() {
  const year = useMonthStore((s) => s.year);
  const month = useMonthStore((s) => s.month);
  const shift = useMonthStore((s) => s.shift);
  return (
    <div className="flex min-w-0 items-center justify-center lg:justify-start lg:gap-1 lg:rounded-lg lg:border lg:border-[var(--mf-border)] lg:bg-white lg:px-1 lg:py-1">
      <Button type="text" size="small" icon={<LeftOutlined />} onClick={() => shift(-1)} aria-label="เดือนก่อนหน้า" />
      <span className="tabular min-w-0 truncate px-1 text-center text-[15px] font-semibold text-[var(--mf-navy)] lg:min-w-[104px] lg:px-0 lg:text-sm lg:font-medium lg:text-[var(--mf-text)]">
        <span className="sm:hidden">{formatShortMonthLabel(year, month)}</span>
        <span className="hidden sm:inline">{formatMonthLabel(year, month)}</span>
      </span>
      <Button type="text" size="small" icon={<RightOutlined />} onClick={() => shift(1)} aria-label="เดือนถัดไป" />
    </div>
  );
}
