import type { ReactNode } from "react";

export function PageHeader({ title, subtitle, extra }: { title: string; subtitle?: string; extra?: ReactNode }) {
  return (
    <div className="mb-6 flex flex-wrap items-start justify-between gap-3">
      <div>
        <h1 className="text-xl font-semibold text-[var(--mf-text)] md:text-2xl">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-[var(--mf-text-secondary)]">{subtitle}</p>}
      </div>
      {extra && <div className="flex items-center gap-2">{extra}</div>}
    </div>
  );
}
