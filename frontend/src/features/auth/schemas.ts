import { z } from "zod";

const passwordComplexity = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/;

export const registerSchema = z.object({
  email: z.string().trim().email("Informe um email válido."),
  password: z
    .string()
    .min(8, "A senha deve ter pelo menos 8 caracteres.")
    .regex(
      passwordComplexity,
      "A senha deve conter letra maiúscula, minúscula, número e caractere especial.",
    ),
  displayName: z.string().trim().min(1, "Informe seu nome."),
});

export type RegisterFormValues = z.infer<typeof registerSchema>;

export const loginSchema = z.object({
  email: z.string().trim().email("Informe um email válido."),
  password: z.string().min(1, "Informe sua senha."),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
