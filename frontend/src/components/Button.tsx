import type { ButtonHTMLAttributes, ReactNode } from "react";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "ghost";
  isLoading?: boolean;
  children: ReactNode;
}

const variantClasses: Record<NonNullable<ButtonProps["variant"]>, string> = {
  primary:
    "bg-brand text-paper hover:bg-brand-strong active:translate-y-px disabled:bg-brand/50",
  secondary:
    "bg-transparent text-ink border border-ink/20 hover:border-ink/40 active:translate-y-px disabled:opacity-50",
  ghost:
    "bg-transparent text-ink-soft hover:text-ink active:translate-y-px disabled:opacity-50",
};

export function Button({
  variant = "primary",
  isLoading = false,
  disabled,
  className = "",
  children,
  ...rest
}: ButtonProps) {
  return (
    <button
      type="button"
      disabled={disabled || isLoading}
      className={`inline-flex items-center justify-center gap-2 rounded-full px-6 py-3 text-sm font-medium whitespace-nowrap transition-colors duration-200 disabled:cursor-not-allowed ${variantClasses[variant]} ${className}`}
      {...rest}
    >
      {isLoading ? (
        <span
          className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent"
          aria-hidden="true"
        />
      ) : null}
      {children}
    </button>
  );
}
