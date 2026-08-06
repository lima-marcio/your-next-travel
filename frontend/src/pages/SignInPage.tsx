import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { AuthLayout } from "../components/AuthLayout";
import { TextField } from "../components/TextField";
import { Button } from "../components/Button";
import { ErrorBanner } from "../components/ErrorBanner";
import { loginSchema, type LoginFormValues } from "../features/auth/schemas";
import { useLoginMutation } from "../features/auth/hooks";
import { useAuthStore } from "../stores/authStore";
import { parseApiError } from "../api/parseApiError";

export function SignInPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const justRegistered = Boolean((location.state as { justRegistered?: boolean } | null)?.justRegistered);
  const setSession = useAuthStore((state) => state.setSession);
  const loginMutation = useLoginMutation();
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null);
    try {
      const response = await loginMutation.mutateAsync(values);
      setSession(response);
      navigate("/dashboard");
    } catch (error) {
      const parsed = parseApiError(error, "Não foi possível entrar. Tente novamente.");
      if (parsed.fieldErrors) {
        for (const [field, message] of Object.entries(parsed.fieldErrors)) {
          setError(field as keyof LoginFormValues, { message });
        }
      }
      setFormError(parsed.message);
    }
  });

  return (
    <AuthLayout
      title="Entrar"
      subtitle="Acesse sua conta pra continuar planejando."
      imageSeed="ynt-signin-porto-coast"
      imageAlt="Costa rochosa com vilarejo à beira-mar"
      footer={
        <>
          Ainda não tem conta?{" "}
          <Link to="/signup" className="font-medium text-brand hover:text-brand-strong">
            Criar conta
          </Link>
        </>
      }
    >
      <form onSubmit={onSubmit} noValidate className="flex flex-col gap-5">
        {justRegistered ? (
          <div
            role="status"
            className="rounded-lg border border-brand/30 bg-brand-soft px-4 py-3 text-sm text-brand-strong"
          >
            Conta criada. Entre com seu email e senha pra continuar.
          </div>
        ) : null}
        {formError ? <ErrorBanner message={formError} /> : null}
        <TextField
          label="Email"
          type="email"
          autoComplete="email"
          error={errors.email?.message}
          {...register("email")}
        />
        <TextField
          label="Senha"
          type="password"
          autoComplete="current-password"
          error={errors.password?.message}
          {...register("password")}
        />
        <Button type="submit" isLoading={isSubmitting} className="mt-2 w-full">
          Entrar
        </Button>
      </form>
    </AuthLayout>
  );
}
