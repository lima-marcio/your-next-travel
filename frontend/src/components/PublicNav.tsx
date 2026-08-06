import { Link } from "react-router-dom";
import { Button } from "./Button";

export function PublicNav() {
  return (
    <header className="sticky top-0 z-40 border-b border-line/70 bg-paper/90 backdrop-blur-sm">
      <nav className="mx-auto flex h-16 max-w-7xl items-center justify-between px-6">
        <Link
          to="/"
          className="font-display text-lg font-semibold tracking-tight text-ink"
        >
          Your Next Travel
        </Link>
        <div className="flex items-center gap-3">
          <Link
            to="/signin"
            className="hidden text-sm font-medium text-ink-soft hover:text-ink sm:inline"
          >
            Entrar
          </Link>
          <Link to="/signup">
            <Button variant="primary" className="px-5 py-2.5">
              Criar conta
            </Button>
          </Link>
        </div>
      </nav>
    </header>
  );
}
