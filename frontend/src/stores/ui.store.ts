import { create } from "zustand";

type UiState = { collapsed: boolean; toggle: () => void };
export const useUiStore = create<UiState>((set) => ({ collapsed: false, toggle: () => set((s) => ({ collapsed: !s.collapsed })) }));
