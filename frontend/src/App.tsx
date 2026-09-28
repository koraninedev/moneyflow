import { ConfigProvider } from "antd";
import thTH from "antd/locale/th_TH";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AppLayout } from "./layout/AppLayout";
import { ProtectedRoute } from "./layout/ProtectedRoute";
import { BudgetsPage } from "./pages/BudgetsPage";
import { CategoriesPage } from "./pages/CategoriesPage";
import { DashboardPage } from "./pages/DashboardPage";
import { ExpensesPage } from "./pages/ExpensesPage";
import { IncomePage } from "./pages/IncomePage";
import { LoginPage } from "./pages/LoginPage";
import { RecurringPage } from "./pages/RecurringPage";
import { ReportsPage } from "./pages/ReportsPage";
import { SavingsPage } from "./pages/SavingsPage";
import { SettingsPage } from "./pages/SettingsPage";
import { colors, fontFamily } from "./theme/tokens";

export default function App() {
  return (
    <ConfigProvider
      locale={thTH}
      theme={{
        token: {
          colorPrimary: colors.primary,
          colorLink: colors.primary,
          colorSuccess: colors.success,
          colorWarning: colors.warning,
          colorError: colors.danger,
          colorBgLayout: colors.bg,
          colorBgContainer: colors.surface,
          colorBorder: colors.border,
          colorText: colors.textPrimary,
          colorTextSecondary: colors.textSecondary,
          colorTextTertiary: colors.textMuted,
          borderRadius: 10,
          fontFamily,
          fontSize: 14
        },
        components: {
          Menu: { itemSelectedBg: colors.primarySoft, itemSelectedColor: colors.primary, itemActiveBg: colors.primarySoft, itemHoverBg: "#F8FAFC", itemBorderRadius: 8 },
          Layout: { headerBg: colors.surface, siderBg: colors.surface, bodyBg: colors.bg, footerBg: colors.surface, headerPadding: "0 16px" },
          Card: { borderRadiusLG: 14 },
          Button: { borderRadius: 8, controlHeight: 38, fontWeight: 500 },
          Table: { headerBg: "#F8FAFC", headerColor: colors.textSecondary, rowHoverBg: "#F8FAFC", borderColor: colors.border },
          Input: { borderRadius: 8, controlHeight: 38 },
          Select: { borderRadius: 8, controlHeight: 38 },
          Modal: { borderRadiusLG: 16 },
          DatePicker: { borderRadius: 8, controlHeight: 38 }
        }
      }}
    >
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<AppLayout />}>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/income" element={<IncomePage />} />
              <Route path="/expenses" element={<ExpensesPage />} />
              <Route path="/budgets" element={<BudgetsPage />} />
              <Route path="/savings" element={<SavingsPage />} />
              <Route path="/categories" element={<CategoriesPage />} />
              <Route path="/recurring" element={<RecurringPage />} />
              <Route path="/reports" element={<ReportsPage />} />
              <Route path="/settings" element={<SettingsPage />} />
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </ConfigProvider>
  );
}
