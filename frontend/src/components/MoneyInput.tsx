import { InputNumber, type InputNumberProps } from "antd";

function formatGrouped(value?: string | number) {
  if (value === undefined || value === null || value === "") return "";
  const raw = String(value);
  const negative = raw.startsWith("-");
  const unsigned = negative ? raw.slice(1) : raw;
  const [intPart, decPart] = unsigned.split(".");
  const grouped = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
  const body = decPart !== undefined ? `${grouped}.${decPart}` : grouped;
  return negative ? `-${body}` : body;
}

function parseGrouped(value?: string) {
  return (value ?? "").replace(/,/g, "");
}

type MoneyInputProps = Omit<InputNumberProps, "formatter" | "parser" | "prefix" | "controls" | "precision" | "decimalSeparator">;

export function MoneyInput({ min = 0.01, className, style, ...props }: MoneyInputProps) {
  return (
    <InputNumber
      {...props}
      className={["w-full mf-money-input", className].filter(Boolean).join(" ")}
      size="large"
      prefix="฿"
      controls={false}
      min={min}
      precision={2}
      decimalSeparator="."
      inputMode="decimal"
      formatter={formatGrouped}
      parser={parseGrouped}
      style={{ width: "100%", ...style }}
    />
  );
}
