import { Button, Form, Select, Switch, Typography, message } from "antd";
import { useMutation } from "@tanstack/react-query";
import { authApi } from "../api/resources";
import { PageHeader } from "../components/PageHeader";
import { th } from "../locales/th";
import { useAuthStore } from "../stores/auth.store";
import { useMonthStore } from "../stores/month.store";
import { DEFAULT_PAYDAY_DAY, periodContaining } from "../lib/pay-cycle";

const dayOptions = Array.from({ length: 28 }, (_, i) => ({ value: i + 1, label: String(i + 1) }));

export function SettingsPage() {
  const user = useAuthStore((s) => s.user);
  const setSession = useAuthStore((s) => s.setSession);
  const token = useAuthStore((s) => s.token);
  const setMonth = useMonthStore((s) => s.setMonth);
  const save = useMutation({
    mutationFn: authApi.updateSettings,
    onSuccess: (updated) => {
      if (token) setSession(token, updated);
      const next = periodContaining(new Date(), updated.periodStartDay, updated.skipWeekendPayday);
      setMonth(next.year, next.month);
      message.success(th.common.saved);
    }
  });

  return (
    <div>
      <PageHeader title={th.settings.title} />
      <div className="mf-card max-w-xl p-5">
        <Typography.Paragraph className="text-[var(--mf-text-secondary)]">{th.settings.intro}</Typography.Paragraph>
        <Form
          layout="vertical"
          key={user?.userId ?? "anon"}
          initialValues={{ periodStartDay: user?.periodStartDay ?? DEFAULT_PAYDAY_DAY, skipWeekendPayday: user?.skipWeekendPayday ?? true }}
          onFinish={(v) => save.mutate(v)}
        >
          <Form.Item name="periodStartDay" label={th.settings.periodStartDay} extra={th.settings.periodStartDayHint}>
            <Select size="large" options={dayOptions} />
          </Form.Item>
          <Form.Item name="skipWeekendPayday" label={th.settings.skipWeekend} valuePropName="checked" extra={th.settings.skipWeekendHint}>
            <Switch />
          </Form.Item>
          <Button type="primary" htmlType="submit" loading={save.isPending} size="large">{th.common.save}</Button>
        </Form>
      </div>
    </div>
  );
}
