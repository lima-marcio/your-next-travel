import { Link } from "react-router-dom";
import {
  ArrowRight,
  CheckCircle,
  Compass,
  Ticket,
} from "@phosphor-icons/react";
import { PublicNav } from "../components/PublicNav";
import { Footer } from "../components/Footer";
import { Button } from "../components/Button";

const guideHighlights = [
  "Clima e melhor época pra viajar",
  "Câmbio local atualizado",
  "Faixa de preço real de hospedagem",
  "Visto, vacinas e exigências de saúde",
  "Orçamento estimado pro seu perfil de viajante",
];

const discoveryHighlights = [
  "Interesses como automobilismo, futebol, leilões, shows e festivais",
  "Horizontes de uma semana, um mês ou um semestre",
  "Modo Passeio Surpresa pra quem topa uma sugestão aleatória",
];

export function LandingPage() {
  return (
    <div className="flex min-h-dvh flex-col">
      <PublicNav />

      <main className="flex-1">
        <section className="mx-auto grid max-w-7xl items-center gap-10 px-6 pt-14 pb-16 md:grid-cols-2 md:pt-20 md:pb-24">
          <div className="flex flex-col gap-6">
            <h1 className="font-display text-4xl leading-tight tracking-tight text-ink md:text-6xl">
              Sua próxima viagem, decidida com dados reais.
            </h1>
            <p className="max-w-[46ch] text-base leading-relaxed text-ink-soft md:text-lg">
              Clima, câmbio, hospedagem e exigências de visto num só lugar,
              ajustados ao seu perfil e ao seu orçamento.
            </p>
            <div className="flex flex-wrap items-center gap-4 pt-2">
              <Link to="/signup">
                <Button variant="primary" className="gap-2 px-7 py-3.5 text-base">
                  Criar conta
                  <ArrowRight size={18} weight="bold" />
                </Button>
              </Link>
              <Link to="/signin" className="text-sm font-medium text-ink-soft hover:text-ink">
                Entrar
              </Link>
            </div>
          </div>
          <div className="overflow-hidden rounded-2xl">
            <img
              src="https://picsum.photos/seed/ynt-hero-lisbon-tram/1200/1400"
              alt="Rua de paralelepípedos em bairro histórico europeu ao entardecer"
              className="h-[420px] w-full object-cover md:h-[520px]"
              loading="eager"
            />
          </div>
        </section>

        <section className="border-t border-line/70 bg-paper-dim">
          <div className="mx-auto grid max-w-7xl items-center gap-10 px-6 py-20 md:grid-cols-2">
            <div className="overflow-hidden rounded-2xl md:order-2">
              <img
                src="https://picsum.photos/seed/ynt-guide-kyoto-street/1200/1000"
                alt="Rua estreita de cidade histórica com lojas tradicionais"
                className="h-[360px] w-full object-cover"
                loading="lazy"
              />
            </div>
            <div className="flex flex-col gap-5 md:order-1">
              <div className="flex items-center gap-3">
                <Compass size={28} weight="light" className="text-brand" />
                <h2 className="font-display text-2xl font-semibold text-ink md:text-3xl">
                  Um retrato completo do destino
                </h2>
              </div>
              <p className="max-w-[52ch] text-ink-soft">
                Antes de fechar a mala, veja o que realmente importa sobre o
                lugar pra onde você vai.
              </p>
              <ul className="flex flex-col gap-3">
                {guideHighlights.map((item) => (
                  <li key={item} className="flex items-start gap-2.5 text-sm text-ink">
                    <CheckCircle size={18} weight="fill" className="mt-0.5 shrink-0 text-brand" />
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </section>

        <section className="border-t border-line/70">
          <div className="mx-auto grid max-w-7xl items-center gap-10 px-6 py-20 md:grid-cols-2">
            <div className="overflow-hidden rounded-2xl">
              <img
                src="https://picsum.photos/seed/ynt-discovery-festival-crowd/1200/1000"
                alt="Multidão em show ao ar livre durante a noite"
                className="h-[360px] w-full object-cover"
                loading="lazy"
              />
            </div>
            <div className="flex flex-col gap-5">
              <div className="flex items-center gap-3">
                <Ticket size={28} weight="light" className="text-brand" />
                <h2 className="font-display text-2xl font-semibold text-ink md:text-3xl">
                  Eventos que combinam com você
                </h2>
              </div>
              <p className="max-w-[52ch] text-ink-soft">
                Registre seus interesses e deixe o app apontar pra onde ir e
                quando.
              </p>
              <ul className="flex flex-col gap-3">
                {discoveryHighlights.map((item) => (
                  <li key={item} className="flex items-start gap-2.5 text-sm text-ink">
                    <CheckCircle size={18} weight="fill" className="mt-0.5 shrink-0 text-brand" />
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </section>

        <section className="border-t border-line/70 bg-ink">
          <div className="mx-auto flex max-w-7xl flex-col items-center gap-6 px-6 py-20 text-center">
            <h2 className="font-display text-3xl font-semibold text-paper md:text-4xl">
              Comece a planejar sua próxima viagem
            </h2>
            <p className="max-w-[42ch] text-paper/70">
              Leva menos de um minuto pra criar sua conta e ver seu primeiro
              guia de destino.
            </p>
            <Link to="/signup">
              <Button variant="primary" className="gap-2 px-7 py-3.5 text-base">
                Criar conta
                <ArrowRight size={18} weight="bold" />
              </Button>
            </Link>
          </div>
        </section>
      </main>

      <Footer />
    </div>
  );
}
