const formatter = new Intl.NumberFormat("th-TH", { style: "currency", currency: "THB" });
export const formatMoney = (value: number | string) => formatter.format(Number(value));
export const chartMoney = (value: unknown) => formatMoney(Number(value ?? 0));
