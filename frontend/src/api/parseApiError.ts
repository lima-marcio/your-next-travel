import { isAxiosError } from "axios";

export interface ParsedApiError {
  message: string;
  fieldErrors?: Record<string, string>;
}

interface ProblemDetailsBody {
  title?: string;
  errors?: Record<string, string[]>;
}

export function parseApiError(error: unknown, fallbackMessage: string): ParsedApiError {
  if (!isAxiosError(error)) {
    return { message: fallbackMessage };
  }

  const status = error.response?.status;
  const body = error.response?.data as ProblemDetailsBody | undefined;

  if (status === 401) {
    return { message: "Email ou senha inválidos." };
  }

  if (body?.errors) {
    const fieldErrors: Record<string, string> = {};
    for (const [field, messages] of Object.entries(body.errors)) {
      const key = field.charAt(0).toLowerCase() + field.slice(1);
      fieldErrors[key] = messages[0];
    }
    return { message: body.title ?? fallbackMessage, fieldErrors };
  }

  return { message: body?.title ?? fallbackMessage };
}
