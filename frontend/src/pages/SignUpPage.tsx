import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { AuthLayout } from "../components/AuthLayout";
import { TextField } from "../components/TextField";
import { Button } from "../components/Button";
import { ErrorBanner } from "../components/ErrorBanner";
import { registerSchema, type RegisterFormValues } from "../features/auth/schemas";
import { useRegisterMutation } from "../features/auth/hooks";
import { parseApiError } from "../api/parseApiError";

export function SignUpPage() {
  const navigate = useNavigate();
  const registerMutation = useRegisterMutation();
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) });

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null);
    try {
      await registerMutation.mutateAsync(values);
      navigate("/signin", { state: { justRegistered: true } });
    } catch (error) {
      const parsed = parseApiError(error, "Não foi possível criar sua conta. Tente novamente.");
      if (parsed.fieldErrors) {
        for (const [field, message] of Object.entries(parsed.fieldErrors)) {
          setError(field as keyof RegisterFormValues, { message });
        }
      }
      setFormError(parsed.message);
    }
  });

  return (
    <AuthLayout
      title="Criar conta"
      subtitle="Leva menos de um minuto pra começar a planejar."
      imageSeed="ynt-signup-andes-trail"
      imageAlt="Trilha de montanha entre picos andinos"
      footer={
        <>
          Já tem conta?{" "}
          <Link to="/signin" className="font-medium text-brand hover:text-brand-strong">
            Entrar
          </Link>
        </>
      }
    >
      <form onSubmit={onSubmit} noValidate className="flex flex-col gap-5">
        {formError ? <ErrorBanner message={formError} /> : null}
        <TextField
          label="Nome"
          type="text"
          autoComplete="name"
          error={errors.displayName?.message}
          {...register("displayName")}
        />
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
          autoComplete="new-password"
          error={errors.password?.message}
          {...register("password")}
        />
        <p className="text-xs text-ink-soft">
          Use pelo menos 8 caracteres, com maiúscula, minúscula, número e
          caractere especial.
        </p>
        <Button type="submit" isLoading={isSubmitting} className="mt-2 w-full">
          Criar conta
        </Button>
      </form>
    </AuthLayout>
  );
}
