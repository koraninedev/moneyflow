const MONTHS_FULL = ["มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม"];
const MONTHS_ABBR = ["ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค."];

export const toBuddhistYear = (year: number) => year + 543;

export const formatMonthLabel = (year: number, month: number) => `${MONTHS_FULL[month - 1]} ${toBuddhistYear(year)}`;

export const formatShortMonthLabel = (year: number, month: number) => `${MONTHS_ABBR[month - 1]} ${toBuddhistYear(year)}`;

export const formatShortDate = (value?: string | Date | null) => {
  if (!value) return "";
  if (typeof value === "string") {
    const dateOnly = value.slice(0, 10);
    const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(dateOnly);
    if (m) {
      const year = Number(m[1]);
      const month = Number(m[2]);
      const day = Number(m[3]);
      return `${day} ${MONTHS_ABBR[month - 1]} ${toBuddhistYear(year)}`;
    }
  }
  const d = typeof value === "string" ? new Date(value) : value;
  if (Number.isNaN(d.getTime())) return "";
  return `${d.getDate()} ${MONTHS_ABBR[d.getMonth()]} ${toBuddhistYear(d.getFullYear())}`;
};
