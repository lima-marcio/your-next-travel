import { MapPinLine, Trash } from "@phosphor-icons/react";
import { InterestsSection } from "../features/interests/InterestsSection";
import { TravelerProfileSection } from "../features/profile/TravelerProfileSection";

export function ProfilePage() {
  return (
    <div className="flex flex-col gap-10">
      <h1 className="font-display text-3xl font-semibold text-ink">Perfil</h1>

      <section>
        <h2 className="font-display text-xl font-semibold text-ink">Localização</h2>
        <div className="mt-4 flex items-center gap-3 rounded-xl border border-dashed border-line px-6 py-8 text-sm text-ink-soft">
          <MapPinLine size={22} weight="light" />
          <p>Em breve: informe seu país e região de origem.</p>
        </div>
      </section>

      <section>
        <h2 className="font-display text-xl font-semibold text-ink">Interesses</h2>
        <p className="mt-1 text-sm text-ink-soft">
          Usados pra encontrar eventos que combinam com você na Descoberta.
        </p>
        <div className="mt-4">
          <InterestsSection />
        </div>
      </section>

      <section>
        <h2 className="font-display text-xl font-semibold text-ink">Perfil de viajante</h2>
        <p className="mt-1 text-sm text-ink-soft">Ajusta o orçamento estimado do guia de destino.</p>
        <div className="mt-4">
          <TravelerProfileSection />
        </div>
      </section>

      <section className="rounded-xl border border-red-200 px-6 py-5">
        <div className="flex items-center gap-3 text-red-700">
          <Trash size={20} weight="light" />
          <div>
            <h2 className="text-sm font-semibold">Excluir conta</h2>
            <p className="text-sm text-red-700/80">Em breve.</p>
          </div>
        </div>
      </section>
    </div>
  );
}
