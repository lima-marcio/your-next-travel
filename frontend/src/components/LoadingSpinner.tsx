export function LoadingSpinner({ label = "Carregando" }: { label?: string }) {
  return (
    <div className="flex items-center justify-center gap-3 py-12 text-ink-soft" role="status">
      <span className="h-5 w-5 animate-spin rounded-full border-2 border-current border-t-transparent" />
      <span className="text-sm">{label}</span>
    </div>
  );
}
