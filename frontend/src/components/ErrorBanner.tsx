import { WarningCircle } from "@phosphor-icons/react";

export function ErrorBanner({ message }: { message: string }) {
  return (
    <div
      role="alert"
      className="flex items-start gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
    >
      <WarningCircle size={18} weight="bold" className="mt-0.5 shrink-0" />
      <span>{message}</span>
    </div>
  );
}
