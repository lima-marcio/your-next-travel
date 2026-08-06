import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthResponse } from "../types/auth";

interface AuthSession {
  token: string;
  expiresAtUtc: string;
  email: string;
  displayName: string;
  role: string;
}

interface AuthState {
  session: AuthSession | null;
  setSession: (response: AuthResponse) => void;
  clearSession: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      session: null,
      setSession: (response) =>
        set({
          session: {
            token: response.token,
            expiresAtUtc: response.expiresAtUtc,
            email: response.email,
            displayName: response.displayName,
            role: response.role,
          },
        }),
      clearSession: () => set({ session: null }),
    }),
    { name: "ynt-auth" },
  ),
);
