import { ArrowDownOutlined, ArrowUpOutlined, BankOutlined, BarChartOutlined, DashboardOutlined, LogoutOutlined, MenuOutlined, PieChartOutlined, PlusOutlined, ReloadOutlined, SettingOutlined, TagsOutlined, WalletOutlined } from "@ant-design/icons";
import { Avatar, Button, Drawer, Dropdown, Layout, Menu } from "antd";
import { useState } from "react";
import { Outlet, useLocation, useNavigate } from "react-router-dom";
import { MonthSwitcher } from "../components/MonthSwitcher";
import { QuickAddSheet } from "../components/QuickAddSheet";
import { th } from "../locales/th";
import { useAuthStore } from "../stores/auth.store";

const navItems = [
  { key: "/", label: th.nav.dashboard, icon: <DashboardOutlined /> },
  { key: "/income", label: th.nav.income, icon: <ArrowDownOutlined /> },
  { key: "/expenses", label: th.nav.expenses, icon: <ArrowUpOutlined /> },
  { key: "/budgets", label: th.nav.budgets, icon: <PieChartOutlined /> },
  { key: "/savings", label: th.nav.savings, icon: <BankOutlined /> }
];
const manageItems = [
  { key: "/categories", label: th.nav.categories, icon: <TagsOutlined /> },
  { key: "/recurring", label: th.nav.recurring, icon: <ReloadOutlined /> },
  { key: "/settings", label: th.nav.settings, icon: <SettingOutlined /> }
];
const analyzeItems = [{ key: "/reports", label: th.nav.reports, icon: <BarChartOutlined /> }];
const allItems = [...navItems, ...manageItems, ...analyzeItems];

const menuItems = [
  ...navItems.map((i) => ({ key: i.key, icon: i.icon, label: i.label })),
  { type: "group" as const, label: th.nav.manageGroup, children: manageItems.map((i) => ({ key: i.key, icon: i.icon, label: i.label })) },
  { type: "group" as const, label: th.nav.analyzeGroup, children: analyzeItems.map((i) => ({ key: i.key, icon: i.icon, label: i.label })) }
];

const accountMenu = (nav: ReturnType<typeof useNavigate>, logout: () => void) => [
  { key: "settings", icon: <SettingOutlined />, label: th.nav.settings, onClick: () => nav("/settings") },
  { key: "out", icon: <LogoutOutlined />, label: th.nav.logout, onClick: () => { logout(); nav("/login"); } }
];

export function AppLayout() {
  const loc = useLocation();
  const nav = useNavigate();
  const logout = useAuthStore((s) => s.logout);
  const user = useAuthStore((s) => s.user);
  const [quick, setQuick] = useState(false);
  const [mobileNav, setMobileNav] = useState(false);
  const [more, setMore] = useState(false);
  const go = (key: string) => nav(key);
  const initials = (user?.displayName ?? "?").trim().slice(0, 1).toUpperCase();
  const tabCls = (active: boolean) =>
    `flex min-w-0 flex-1 flex-col items-center gap-0.5 py-1 text-[11px] font-medium ${active ? "text-[var(--mf-primary)]" : "text-[var(--mf-text-secondary)]"}`;
  const moreActive = !["/", "/budgets", "/savings"].includes(loc.pathname);

  const sidebarContent = (
    <>
      <div className="flex items-center gap-2 px-5 py-5">
        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-[var(--mf-primary)] text-white">
          <WalletOutlined />
        </div>
        <span className="text-lg font-semibold tracking-tight text-[var(--mf-navy)]">{th.brand}</span>
      </div>
      <Menu mode="inline" selectedKeys={[loc.pathname]} items={menuItems} onClick={(e) => go(e.key)} className="border-none px-2" style={{ background: "transparent" }} />
    </>
  );

  return (
    <Layout className="min-h-screen" style={{ background: "var(--mf-bg)" }}>
      <Layout.Sider
        width={252}
        className="hidden border-r border-[var(--mf-border)] lg:block"
        theme="light"
        style={{ background: "var(--mf-surface)", position: "sticky", top: 0, height: "100vh", overflow: "auto" }}
      >
        {sidebarContent}
      </Layout.Sider>
      <Layout>
        <Layout.Header
          className="mf-app-header sticky top-0 z-30 flex h-14 items-center border-b border-[var(--mf-border)] px-3 lg:h-16 lg:px-6"
          style={{ background: "var(--mf-surface)", lineHeight: "normal" }}
        >
          <div className="flex min-w-0 flex-1 items-center lg:hidden">
            <button type="button" className="mr-1 flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-[var(--mf-navy)]" aria-label={th.nav.more} onClick={() => setMobileNav(true)}>
              <MenuOutlined style={{ fontSize: 18 }} />
            </button>
            <div className="min-w-0 flex-1">
              <MonthSwitcher />
            </div>
            <Dropdown menu={{ items: accountMenu(nav, logout) }} placement="bottomRight">
              <button type="button" className="ml-1 flex h-9 w-9 shrink-0 items-center justify-center rounded-full" aria-label={th.nav.account}>
                <Avatar size={32} style={{ background: "var(--mf-navy-soft)" }}>{initials}</Avatar>
              </button>
            </Dropdown>
          </div>
          <div className="hidden min-w-0 w-full items-center justify-between lg:flex">
            <MonthSwitcher />
            <div className="flex shrink-0 items-center gap-2">
              <Button type="primary" ghost icon={<PlusOutlined />} onClick={() => setQuick(true)}>
                {th.quickAdd.button}
              </Button>
              <Dropdown menu={{ items: accountMenu(nav, logout) }} placement="bottomRight">
                <Button type="text" className="flex items-center gap-2 px-2">
                  <Avatar size={28} style={{ background: "var(--mf-navy-soft)" }}>{initials}</Avatar>
                  <span className="text-sm font-medium text-[var(--mf-text)]">{user?.displayName ?? th.nav.account}</span>
                </Button>
              </Dropdown>
            </div>
          </div>
        </Layout.Header>
        <Layout.Content className="mx-auto w-full max-w-[1440px] px-4 py-6 pb-28 sm:px-5 md:px-6 lg:px-8 lg:pb-8">
          <Outlet />
        </Layout.Content>
        <nav className="fixed bottom-0 left-0 right-0 z-20 border-t border-[var(--mf-border)] bg-white/95 pb-[env(safe-area-inset-bottom)] backdrop-blur lg:hidden" aria-label={th.brand}>
          <div className="flex items-end px-2 pt-1">
            <button type="button" className={tabCls(loc.pathname === "/")} onClick={() => go("/")}>
              <DashboardOutlined style={{ fontSize: 20 }} />
              {th.nav.dashboard}
            </button>
            <button type="button" className={tabCls(loc.pathname === "/budgets")} onClick={() => go("/budgets")}>
              <PieChartOutlined style={{ fontSize: 20 }} />
              {th.nav.budgets}
            </button>
            <div className="flex w-14 shrink-0 flex-col items-center">
              <button type="button" className="-mt-5 flex h-12 w-12 items-center justify-center rounded-full bg-[var(--mf-primary)] text-white shadow-md" onClick={() => setQuick(true)} aria-label={th.quickAdd.button}>
                <PlusOutlined style={{ fontSize: 20 }} />
              </button>
            </div>
            <button type="button" className={tabCls(loc.pathname === "/savings")} onClick={() => go("/savings")}>
              <BankOutlined style={{ fontSize: 20 }} />
              {th.nav.savings}
            </button>
            <button type="button" className={tabCls(moreActive)} onClick={() => setMore(true)}>
              <MenuOutlined style={{ fontSize: 20 }} />
              {th.nav.more}
            </button>
          </div>
        </nav>
      </Layout>
      <QuickAddSheet open={quick} onClose={() => setQuick(false)} />
      <Drawer title={th.brand} placement="left" open={mobileNav} onClose={() => setMobileNav(false)} width={272} styles={{ body: { padding: 0 } }}>
        <Menu mode="inline" selectedKeys={[loc.pathname]} items={menuItems} onClick={(e) => { go(e.key); setMobileNav(false); }} className="border-none" />
        <div className="px-4 py-3">
          <Button danger block icon={<LogoutOutlined />} onClick={() => { logout(); nav("/login"); }}>{th.nav.logout}</Button>
        </div>
      </Drawer>
      <Drawer title={th.nav.more} placement="bottom" open={more} onClose={() => setMore(false)} height="auto">
        <div className="grid grid-cols-3 gap-3">
          {allItems.map((i) => (
            <button key={i.key} className="flex flex-col items-center gap-1 rounded-lg border border-[var(--mf-border)] py-3 text-xs text-[var(--mf-text)]" onClick={() => { go(i.key); setMore(false); }}>
              <span style={{ fontSize: 18 }}>{i.icon}</span>
              {i.label}
            </button>
          ))}
        </div>
        <Button danger block className="mt-4" icon={<LogoutOutlined />} onClick={() => { logout(); nav("/login"); }}>{th.nav.logout}</Button>
      </Drawer>
    </Layout>
  );
}
