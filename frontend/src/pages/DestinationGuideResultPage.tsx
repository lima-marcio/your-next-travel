import type { ReactNode } from "react";
import { useParams } from "react-router-dom";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { ErrorState } from "../components/ErrorState";
import { useDestinationGuideDetail } from "../features/destinationGuide/hooks";
import { interestCategoryLabels } from "../features/interests/labels";

export function DestinationGuideResultPage() {
  const { searchId } = useParams<{ searchId: string }>();
  const { data, isLoading, isError, refetch } = useDestinationGuideDetail(searchId);

  if (isLoading) {
    return <LoadingSpinner label="Carregando guia de destino" />;
  }

  if (isError || !data) {
    return <ErrorState onRetry={() => refetch()} />;
  }

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="font-display text-3xl font-semibold text-ink">
          {data.cityName}, {data.countryName}
        </h1>
        <p className="mt-1 text-sm text-ink-soft">
          {data.startDate} – {data.endDate}
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <SummaryCard title="Clima">
          {data.weather.available ? (
            <>
              <p className="text-sm text-ink-soft">
                {data.weather.isForecast ? "Previsão" : "Média histórica"}
              </p>
              <p className="mt-2 font-display text-2xl text-ink">
                {data.weather.avgTempC?.toFixed(1)}°C
              </p>
              <p className="text-sm text-ink-soft">
                Mín {data.weather.minTempC?.toFixed(1)}°C · Máx {data.weather.maxTempC?.toFixed(1)}°C
              </p>
            </>
          ) : (
            <p className="text-sm text-ink-soft">Dado de clima indisponível pra esse destino.</p>
          )}
        </SummaryCard>

        <SummaryCard title="Câmbio">
          {data.currency.available ? (
            <p className="text-sm text-ink">
              1 {data.currency.homeCurrencyCode} = {data.currency.homeToLocalRate?.toFixed(4)}{" "}
              {data.currency.localCurrencyCode}
            </p>
          ) : (
            <p className="text-sm text-ink-soft">Câmbio indisponível no momento.</p>
          )}
        </SummaryCard>

        <SummaryCard title="Hospedagem">
          {data.lodging.available ? (
            <p className="text-sm text-ink">
              Média {data.lodging.avgNightlyAmount?.toFixed(2)} {data.lodging.currency} / noite
              <br />
              <span className="text-ink-soft">
                Faixa {data.lodging.minNightlyAmount?.toFixed(2)} – {data.lodging.maxNightlyAmount?.toFixed(2)}
              </span>
            </p>
          ) : (
            <p className="text-sm text-ink-soft">Sem estimativa de hospedagem em cache ainda.</p>
          )}
          <p className="mt-3 text-xs text-ink-soft/80">
            Valor estimado — o valor definitivo será confirmado diretamente com a empresa
            selecionada.
          </p>
        </SummaryCard>

        <SummaryCard title="Visto e saúde">
          {data.legalHealth.available ? (
            <div className="flex flex-col gap-2 text-sm text-ink">
              {data.legalHealth.visaRequirementText ? <p>{data.legalHealth.visaRequirementText}</p> : null}
              {data.legalHealth.vaccinationRequirementText ? (
                <p>{data.legalHealth.vaccinationRequirementText}</p>
              ) : null}
            </div>
          ) : (
            <p className="text-sm text-ink-soft">Sem exigências cadastradas pra esse país ainda.</p>
          )}
        </SummaryCard>
      </div>

      <SummaryCard title="Orçamento estimado">
        <p className="font-display text-2xl text-ink">
          {data.budget.totalAmount.toFixed(2)} {data.budget.currency}
        </p>
        <p className="mt-1 text-sm text-ink-soft">{data.budget.assumptionsNote}</p>
        <p className="mt-3 text-xs text-ink-soft/80">
          Valor estimado — o valor definitivo será confirmado diretamente com a empresa
          selecionada.
        </p>
      </SummaryCard>

      <div>
        <h2 className="font-display text-xl font-semibold text-ink">Eventos compatíveis</h2>
        <div className="mt-4">
          {data.matchingEvents.length > 0 ? (
            <ul className="flex flex-col gap-3">
              {data.matchingEvents.map((event) => (
                <li key={event.id} className="rounded-lg border border-line/70 px-4 py-3 text-sm">
                  <p className="font-medium text-ink">{event.title}</p>
                  <p className="text-ink-soft">
                    {interestCategoryLabels[event.category]}
                    {event.venueName ? ` · ${event.venueName}` : ""}
                  </p>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-sm text-ink-soft">
              Nenhum evento compatível com seus interesses nesse período.
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

function SummaryCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="rounded-xl border border-line/70 p-6">
      <h2 className="font-display text-lg font-semibold text-ink">{title}</h2>
      <div className="mt-3">{children}</div>
    </section>
  );
}
