import { Link } from "react-router-dom";
import type { ReactNode } from "react";

interface AuthLayoutProps {
  title: string;
  subtitle: string;
  imageSeed: string;
  imageAlt: string;
  children: ReactNode;
  footer: ReactNode;
}

export function AuthLayout({ title, subtitle, imageSeed, imageAlt, children, footer }: AuthLayoutProps) {
  return (
    <div className="grid min-h-dvh md:grid-cols-2">
      <div className="hidden md:block">
        <img
          src={`https://picsum.photos/seed/${imageSeed}/1000/1400`}
          alt={imageAlt}
          className="h-full w-full object-cover"
        />
      </div>
      <div className="flex flex-col justify-center px-6 py-16 sm:px-12 lg:px-20">
        <Link to="/" className="mb-10 font-display text-lg font-semibold text-ink">
          Your Next Travel
        </Link>
        <div className="mx-auto w-full max-w-sm">
          <h1 className="font-display text-3xl font-semibold text-ink">{title}</h1>
          <p className="mt-2 text-sm text-ink-soft">{subtitle}</p>
          <div className="mt-8">{children}</div>
          <div className="mt-6 text-sm text-ink-soft">{footer}</div>
        </div>
      </div>
    </div>
  );
}
