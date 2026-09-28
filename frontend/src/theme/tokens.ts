export const colors = {
  primary: "#2563EB",
  primaryHover: "#1D4ED8",
  primaryActive: "#1E40AF",
  primarySoft: "#EFF6FF",
  navy: "#0B1F33",
  navySoft: "#0F2742",
  bg: "#F5F7FB",
  surface: "#FFFFFF",
  border: "#E2E8F0",
  borderStrong: "#CBD5E1",
  textPrimary: "#0F172A",
  textSecondary: "#64748B",
  textMuted: "#94A3B8",
  success: "#16A34A",
  successSoft: "#ECFDF5",
  warning: "#D97706",
  warningSoft: "#FFFBEB",
  danger: "#DC2626",
  dangerSoft: "#FEF2F2"
} as const;

export const chartColors = {
  income: colors.success,
  expenses: colors.danger,
  savings: colors.primary,
  remaining: colors.primary,
  fixed: colors.navySoft,
  variable: colors.primary,
  sequence: ["#2563EB", "#0F2742", "#3B82F6", "#64748B", "#1D4ED8", "#94A3B8"]
} as const;

export const fontFamily = "'Noto Sans Thai', 'Inter', system-ui, -apple-system, sans-serif";
