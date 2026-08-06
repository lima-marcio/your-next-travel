import { beforeEach, describe, expect, it } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { SignUpPage } from "../../../pages/SignUpPage";
import { SignInPage } from "../../../pages/SignInPage";
import { useAuthStore } from "../../../stores/authStore";

function renderAuthRoutes(initialPath: string) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>
        <Routes>
          <Route path="/signup" element={<SignUpPage />} />
          <Route path="/signin" element={<SignInPage />} />
          <Route path="/dashboard" element={<div>DASHBOARD_PLACEHOLDER</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("auth flow", () => {
  beforeEach(() => {
    useAuthStore.getState().clearSession();
  });

  it("redirects from sign up to sign in with a confirmation, without logging in", async () => {
    const user = userEvent.setup();
    renderAuthRoutes("/signup");

    await user.type(screen.getByLabelText("Nome"), "Ana Viajante");
    await user.type(screen.getByLabelText("Email"), "ana@example.com");
    await user.type(screen.getByLabelText("Senha"), "Senha123!");
    await user.click(screen.getByRole("button", { name: "Criar conta" }));

    await waitFor(() =>
      expect(screen.getByRole("heading", { name: "Entrar" })).toBeInTheDocument(),
    );
    expect(screen.getByText(/Conta criada/)).toBeInTheDocument();
    expect(useAuthStore.getState().session).toBeNull();
  });

  it("signs in successfully and stores the session", async () => {
    const user = userEvent.setup();
    renderAuthRoutes("/signin");

    await user.type(screen.getByLabelText("Email"), "ana@example.com");
    await user.type(screen.getByLabelText("Senha"), "Senha123!");
    await user.click(screen.getByRole("button", { name: "Entrar" }));

    await waitFor(() => expect(screen.getByText("DASHBOARD_PLACEHOLDER")).toBeInTheDocument());
    expect(useAuthStore.getState().session?.email).toBe("ana@example.com");
  });

  it("shows an inline error on invalid credentials", async () => {
    const user = userEvent.setup();
    renderAuthRoutes("/signin");

    await user.type(screen.getByLabelText("Email"), "ana@example.com");
    await user.type(screen.getByLabelText("Senha"), "wrongpass");
    await user.click(screen.getByRole("button", { name: "Entrar" }));

    expect(await screen.findByText("Email ou senha inválidos.")).toBeInTheDocument();
    expect(useAuthStore.getState().session).toBeNull();
  });

  it("shows client-side validation errors for a weak password", async () => {
    const user = userEvent.setup();
    renderAuthRoutes("/signup");

    await user.type(screen.getByLabelText("Nome"), "Ana Viajante");
    await user.type(screen.getByLabelText("Email"), "ana@example.com");
    await user.type(screen.getByLabelText("Senha"), "123");
    await user.click(screen.getByRole("button", { name: "Criar conta" }));

    expect(await screen.findByText("A senha deve ter pelo menos 8 caracteres.")).toBeInTheDocument();
  });
});
