import { useEffect } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { getToken } from "../api/client";
import { useAuthStore } from "../stores/auth.store";

export function ProtectedRoute() {
  const hydrate = useAuthStore((s) => s.hydrate);
  const token = getToken();
  useEffect(() => { if (token) void hydrate(); }, [token, hydrate]);
  return token ? <Outlet /> : <Navigate to="/login" replace />;
}
