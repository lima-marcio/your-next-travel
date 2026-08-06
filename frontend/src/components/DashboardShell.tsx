import { useEffect, useRef, useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import {
  CaretLeft,
  CaretRight,
  Compass,
  House,
  SignOut,
  Ticket,
  UserCircle,
} from "@phosphor-icons/react";
import { useAuthStore } from "../stores/authStore";

const navItems = [
  { to: "/dashboard", label: "Painel", icon: House },
  { to: "/guide", label: "Guia de destino", icon: Compass },
  { to: "/discovery", label: "Descoberta", icon: Ticket },
  { to: "/profile", label: "Perfil", icon: UserCircle },
];

const SIDEBAR_STORAGE_KEY = "ynt-sidebar-collapsed";

export function DashboardShell() {
  const [collapsed, setCollapsed] = useState(
    () => localStorage.getItem(SIDEBAR_STORAGE_KEY) === "1",
  );

  useEffect(() => {
    localStorage.setItem(SIDEBAR_STORAGE_KEY, collapsed ? "1" : "0");
  }, [collapsed]);

  return (
    <div className="flex min-h-dvh">
      <aside
        className={`hidden shrink-0 border-r border-line/70 bg-paper-dim transition-[width] duration-200 md:flex md:flex-col ${
          collapsed ? "w-20" : "w-64"
        }`}
      >
        <div className="flex h-16 items-center justify-between px-4">
          {!collapsed && (
            <span className="font-display text-base font-semibold text-ink">
              Your Next Travel
            </span>
          )}
          <button
            type="button"
            onClick={() => setCollapsed((value) => !value)}
            aria-label={collapsed ? "Expandir menu" : "Recolher menu"}
            className="rounded-full p-1.5 text-ink-soft hover:bg-paper hover:text-ink"
          >
            {collapsed ? <CaretRight size={18} /> : <CaretLeft size={18} />}
          </button>
        </div>
        <nav className="flex flex-col gap-1 px-3 py-2">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors ${
                  isActive
                    ? "bg-brand-soft text-brand-strong"
                    : "text-ink-soft hover:bg-paper hover:text-ink"
                }`
              }
              title={collapsed ? label : undefined}
            >
              <Icon size={20} weight="regular" />
              {!collapsed && <span>{label}</span>}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <Navbar />
        <main className="flex-1 bg-paper px-6 py-8 md:px-10">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

function Navbar() {
  const [open, setOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const session = useAuthStore((state) => state.session);
  const clearSession = useAuthStore((state) => state.clearSession);
  const navigate = useNavigate();

  useEffect(() => {
    if (!open) return;
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [open]);

  const initial = session?.displayName.trim().charAt(0).toUpperCase() || "?";

  function handleLogout() {
    // No POST /api/auth/logout yet (backend spec item 1.4 not implemented) —
    // clearing the local session is the full effect for now.
    clearSession();
    navigate("/signin");
  }

  return (
    <header className="flex h-16 items-center justify-end border-b border-line/70 px-6 md:px-10">
      <div className="relative" ref={menuRef}>
        <button
          type="button"
          onClick={() => setOpen((value) => !value)}
          className="flex h-9 w-9 items-center justify-center rounded-full bg-brand text-sm font-semibold text-paper"
          aria-haspopup="menu"
          aria-expanded={open}
        >
          {initial}
        </button>
        {open ? (
          <div
            role="menu"
            className="absolute right-0 mt-2 w-44 rounded-lg border border-line/70 bg-paper py-1 shadow-lg"
          >
            <NavLink
              to="/profile"
              role="menuitem"
              className="block px-4 py-2 text-sm text-ink hover:bg-paper-dim"
              onClick={() => setOpen(false)}
            >
              Perfil
            </NavLink>
            <button
              type="button"
              role="menuitem"
              onClick={handleLogout}
              className="flex w-full items-center gap-2 px-4 py-2 text-left text-sm text-ink hover:bg-paper-dim"
            >
              <SignOut size={16} />
              Sair
            </button>
          </div>
        ) : null}
      </div>
    </header>
  );
}
