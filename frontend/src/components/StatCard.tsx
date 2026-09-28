import { Card } from "antd";
import { formatMoney } from "../lib/money";

const toneClass: Record<string, string> = {
  pos: "text-[var(--mf-success)]",
  neg: "text-[var(--mf-danger)]",
  neutral: "text-[var(--mf-text)]",
  primary: "text-[var(--mf-primary)]"
};

export function StatCard({ label, value, tone = "neutral", emphasis }: { label: string; value: number; tone?: "pos" | "neg" | "neutral" | "primary"; emphasis?: boolean }) {
  return (
    <Card className="mf-card h-full" styles={{ body: { padding: emphasis ? 20 : 16 } }} style={emphasis ? { background: "var(--mf-navy-soft)", borderColor: "var(--mf-navy-soft)" } : undefined}>
      <div className={`text-sm font-medium ${emphasis ? "text-white/70" : "text-[var(--mf-text-secondary)]"}`}>{label}</div>
      <div className={`tabular mt-1.5 font-semibold ${emphasis ? "text-2xl text-white md:text-3xl" : `text-xl ${toneClass[tone]}`}`}>{formatMoney(value)}</div>
    </Card>
  );
}
