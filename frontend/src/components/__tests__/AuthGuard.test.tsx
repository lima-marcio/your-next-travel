import { afterEach, describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { AuthGuard } from "../AuthGuard";
import { useAuthStore } from "../../stores/authStore";

function renderGuardedRoute() {
  return render(
    <MemoryRouter initialEntries={["/dashboard"]}>
      <Routes>
        <Route path="/signin" element={<div>SIGNIN_PAGE</div>} />
        <Route element={<AuthGuard />}>
          <Route path="/dashboard" element={<div>DASHBOARD_PAGE</div>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

describe("AuthGuard", () => {
  afterEach(() => {
    useAuthStore.getState().clearSession();
  });

  it("redirects to sign in when there is no session", () => {
    renderGuardedRoute();

    expect(screen.getByText("SIGNIN_PAGE")).toBeInTheDocument();
    expect(screen.queryByText("DASHBOARD_PAGE")).not.toBeInTheDocument();
  });

  it("renders the protected route when a session exists", () => {
    useAuthStore.getState().setSession({
      token: "fake-token",
      expiresAtUtc: new Date(Date.now() + 7_200_000).toISOString(),
      email: "ana@example.com",
      displayName: "Ana Viajante",
      role: "User",
    });

    renderGuardedRoute();

    expect(screen.getByText("DASHBOARD_PAGE")).toBeInTheDocument();
  });
});
