import { forwardRef, type InputHTMLAttributes } from "react";

interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export const TextField = forwardRef<HTMLInputElement, TextFieldProps>(
  ({ label, error, id, className = "", ...rest }, ref) => {
    const inputId = id ?? rest.name;
    return (
      <div className="flex flex-col gap-2">
        <label htmlFor={inputId} className="text-sm font-medium text-ink">
          {label}
        </label>
        <input
          ref={ref}
          id={inputId}
          className={`rounded-lg border bg-paper px-4 py-3 text-sm text-ink placeholder:text-ink-soft/60 focus:outline-none focus:ring-2 focus:ring-brand/40 ${
            error ? "border-red-400" : "border-line"
          } ${className}`}
          aria-invalid={Boolean(error)}
          aria-describedby={error ? `${inputId}-error` : undefined}
          {...rest}
        />
        {error ? (
          <p id={`${inputId}-error`} className="text-sm text-red-600">
            {error}
          </p>
        ) : null}
      </div>
    );
  },
);

TextField.displayName = "TextField";
