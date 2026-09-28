const BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000/api";

export class ApiError extends Error {
  constructor(public status: number, public code: string, message: string, public fields?: Record<string, string>) {
    super(message);
  }
}

export function getToken() {
  return localStorage.getItem("mf_token");
}

export function setToken(token: string | null) {
  if (token) localStorage.setItem("mf_token", token);
  else {
    localStorage.removeItem("mf_token");
    localStorage.removeItem("mf_user");
  }
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  headers.set("Accept", "application/json");
  if (init.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json");
  const token = getToken();
  if (token) headers.set("Authorization", `Bearer ${token}`);
  const res = await fetch(`${BASE}${path}`, { ...init, headers });
  if (res.status === 401) {
    setToken(null);
    if (!path.startsWith("/auth/")) window.location.assign("/login");
    throw new ApiError(401, "UNAUTHORIZED", "Not authenticated");
  }
  if (res.status === 204) return undefined as T;
  const json = await res.json().catch(() => ({}));
  if (!res.ok) {
    const err = json?.error ?? {};
    throw new ApiError(res.status, err.code ?? "ERROR", err.message ?? res.statusText, err.fields);
  }
  return (json.data ?? json) as T;
}

export const apiGet = <T>(path: string) => api<T>(path);
export const apiPost = <T>(path: string, body: unknown) => api<T>(path, { method: "POST", body: JSON.stringify(body) });
export const apiPut = <T>(path: string, body: unknown) => api<T>(path, { method: "PUT", body: JSON.stringify(body) });
export const apiPatch = <T>(path: string, body: unknown) => api<T>(path, { method: "PATCH", body: JSON.stringify(body) });
export const apiDelete = (path: string) => api<void>(path, { method: "DELETE" });
