import { Button, Empty } from "antd";

export function EmptyState({ description, cta, onClick }: { description: string; cta?: string; onClick?: () => void }) {
  return (
    <div className="flex flex-col items-center gap-4 rounded-2xl border border-dashed border-[var(--mf-border-strong)] bg-white/60 px-6 py-14 text-center">
      <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={<span className="text-[var(--mf-text-secondary)]">{description}</span>} />
      {cta && <Button type="primary" onClick={onClick}>{cta}</Button>}
    </div>
  );
}
