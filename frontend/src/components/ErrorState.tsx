import { ArrowClockwise, WarningCircle } from "@phosphor-icons/react";
import { Button } from "./Button";

interface ErrorStateProps {
  message?: string;
  onRetry?: () => void;
}

export function ErrorState({ message = "Algo deu errado. Tente novamente.", onRetry }: ErrorStateProps) {
  return (
    <div className="flex flex-col items-center gap-4 rounded-xl border border-line/70 px-6 py-12 text-center">
      <WarningCircle size={28} weight="light" className="text-ink-soft" />
      <p className="text-sm text-ink-soft">{message}</p>
      {onRetry ? (
        <Button variant="secondary" onClick={onRetry} className="gap-2 px-5 py-2.5">
          <ArrowClockwise size={16} />
          Tentar novamente
        </Button>
      ) : null}
    </div>
  );
}
