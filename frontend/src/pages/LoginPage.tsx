import { LockOutlined, MailOutlined, UserOutlined, WalletOutlined } from "@ant-design/icons";
import { Button, Card, Form, Input, Typography, message } from "antd";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { authApi } from "../api/resources";
import { toThaiErrorMessage } from "../lib/errors";
import { th } from "../locales/th";
import { useAuthStore } from "../stores/auth.store";

export function LoginPage() {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [loading, setLoading] = useState(false);
  const setSession = useAuthStore((s) => s.setSession);
  const nav = useNavigate();

  const onFinish = async (v: { email: string; password: string; displayName?: string }) => {
    setLoading(true);
    try {
      if (mode === "register") {
        await authApi.register(v.email, v.password, v.displayName ?? v.email);
      }
      const res = await authApi.login(v.email, v.password);
      setSession(res.token, res.user);
      nav("/");
    } catch (e) {
      message.error(toThaiErrorMessage(e));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-[var(--mf-bg)] p-4">
      <Card className="mf-card w-full max-w-md" styles={{ body: { padding: 32 } }}>
        <div className="mb-6 flex flex-col items-center text-center">
          <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-[var(--mf-primary)] text-xl text-white">
            <WalletOutlined />
          </div>
          <Typography.Title level={3} className="!mb-1 !leading-tight !text-[var(--mf-navy)]">
            <span className="block">{th.brandLockup.line1}</span>
            <span className="block">{th.brandLockup.line2}</span>
          </Typography.Title>
          <Typography.Text className="text-[var(--mf-text-secondary)]">{th.tagline}</Typography.Text>
        </div>
        <Form layout="vertical" onFinish={onFinish} requiredMark={false}>
          {mode === "register" && (
            <Form.Item name="displayName" label={th.login.displayName} rules={[{ required: true }]}>
              <Input size="large" prefix={<UserOutlined className="text-[var(--mf-text-muted)]" />} />
            </Form.Item>
          )}
          <Form.Item name="email" label={th.login.email} rules={[{ required: true, type: "email" }]}>
            <Input size="large" prefix={<MailOutlined className="text-[var(--mf-text-muted)]" />} />
          </Form.Item>
          <Form.Item name="password" label={th.login.password} rules={[{ required: true, min: 8 }]}>
            <Input.Password size="large" prefix={<LockOutlined className="text-[var(--mf-text-muted)]" />} />
          </Form.Item>
          <Button type="primary" htmlType="submit" loading={loading} block size="large">
            {mode === "login" ? th.login.login : th.login.register}
          </Button>
        </Form>
        <Button type="link" className="mt-3 w-full px-0 text-center" onClick={() => setMode(mode === "login" ? "register" : "login")}>
          {mode === "login" ? th.login.toRegister : th.login.toLogin}
        </Button>
      </Card>
    </div>
  );
}
