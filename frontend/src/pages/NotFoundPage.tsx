import { Link } from "react-router-dom";
import { Button } from "../components/Button";

export function NotFoundPage() {
  return (
    <div className="flex min-h-dvh flex-col items-center justify-center gap-4 px-6 text-center">
      <h1 className="font-display text-3xl font-semibold text-ink">Página não encontrada</h1>
      <p className="max-w-[42ch] text-ink-soft">
        Essa área ainda está sendo construída ou o endereço não existe.
      </p>
      <Link to="/">
        <Button variant="secondary">Voltar ao início</Button>
      </Link>
    </div>
  );
}
