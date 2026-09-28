import { create } from "zustand";
import { authApi } from "../api/resources";
import { getToken, setToken } from "../api/client";
import type { User } from "../api/types";
import { useMonthStore } from "./month.store";

const USER_KEY = "mf_user";

function readStoredUser(): User | null {
  try {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as User) : null;
  } catch {
    return null;
  }
}

function writeStoredUser(user: User | null) {
  if (user) localStorage.setItem(USER_KEY, JSON.stringify(user));
  else localStorage.removeItem(USER_KEY);
}

type AuthState = {
  user: User | null;
  token: string | null;
  setSession: (token: string, user: User) => void;
  hydrate: () => Promise<void>;
  logout: () => void;
};

export const useAuthStore = create<AuthState>((set) => ({
  user: readStoredUser(),
  token: getToken(),
  setSession: (token, user) => {
    setToken(token);
    writeStoredUser(user);
    set({ token, user });
    useMonthStore.getState().syncFromPayday(user.periodStartDay, user.skipWeekendPayday);
  },
  hydrate: async () => {
    if (!getToken()) return;
    try {
      const user = await authApi.me();
      writeStoredUser(user);
      set({ token: getToken(), user });
      useMonthStore.getState().syncFromPayday(user.periodStartDay, user.skipWeekendPayday);
    } catch {
      /* 401 เคลียร์ token ใน client แล้ว; network error คงชื่อที่ persist ไว้ */
    }
  },
  logout: () => { setToken(null); writeStoredUser(null); set({ token: null, user: null }); }
}));
