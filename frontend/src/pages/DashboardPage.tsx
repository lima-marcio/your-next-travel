import { Link } from "react-router-dom";
import { ArrowRight, Shuffle } from "@phosphor-icons/react";
import { Button } from "../components/Button";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { ErrorState } from "../components/ErrorState";
import { EmptyState } from "../components/EmptyState";
import { useDestinationGuideHistory } from "../features/destinationGuide/hooks";
import { useDiscoveryFeed, useRandomOutingMutation } from "../features/discovery/hooks";

export function DashboardPage() {
  const historyQuery = useDestinationGuideHistory();
  const feedQuery = useDiscoveryFeed();
  const randomOutingMutation = useRandomOutingMutation();

  const mostRecentSearch = historyQuery.data?.[0];
  const upcomingSuggestions =
    feedQuery.data?.groups.flatMap((group) => group.suggestions).slice(0, 2) ?? [];

  return (
    <div className="flex flex-col gap-8">
      <h1 className="font-display text-3xl font-semibold text-ink">Painel</h1>

      <div className="grid gap-6 md:grid-cols-2">
        <section className="rounded-xl border border-line/70 p-6">
          <h2 className="font-display text-xl font-semibold text-ink">Guia de destino</h2>
          <div className="mt-4">
            {historyQuery.isLoading ? (
              <LoadingSpinner label="Carregando histórico" />
            ) : historyQuery.isError ? (
              <ErrorState onRetry={() => historyQuery.refetch()} />
            ) : mostRecentSearch ? (
              <div className="flex flex-col gap-3">
                <p className="text-sm text-ink-soft">
                  Última busca:{" "}
                  <span className="font-medium text-ink">{mostRecentSearch.cityName}</span>
                </p>
                <Link
                  to={`/guide/${mostRecentSearch.searchId}`}
                  className="inline-flex w-fit items-center gap-2 text-sm font-medium text-brand hover:text-brand-strong"
                >
                  Ver guia completo
                  <ArrowRight size={16} />
                </Link>
              </div>
            ) : (
              <EmptyState
                title="Nenhuma busca ainda"
                description="Monte seu primeiro guia de destino."
                action={
                  <Link to="/guide">
                    <Button variant="secondary" className="px-5 py-2.5">
                      Criar guia
                    </Button>
                  </Link>
                }
              />
            )}
          </div>
        </section>

        <section className="rounded-xl border border-line/70 p-6">
          <h2 className="font-display text-xl font-semibold text-ink">Descoberta</h2>
          <div className="mt-4">
            {feedQuery.isLoading ? (
              <LoadingSpinner label="Carregando eventos" />
            ) : feedQuery.isError ? (
              <ErrorState onRetry={() => feedQuery.refetch()} />
            ) : upcomingSuggestions.length > 0 ? (
              <div className="flex flex-col gap-3">
                <ul className="flex flex-col gap-2">
                  {upcomingSuggestions.map((suggestion) => (
                    <li key={suggestion.eventId} className="text-sm text-ink">
                      <span className="font-medium">{suggestion.title}</span>
                      {suggestion.cityName ? (
                        <span className="text-ink-soft"> · {suggestion.cityName}</span>
                      ) : null}
                    </li>
                  ))}
                </ul>
                <Link
                  to="/discovery"
                  className="inline-flex w-fit items-center gap-2 text-sm font-medium text-brand hover:text-brand-strong"
                >
                  Ver todos
                  <ArrowRight size={16} />
                </Link>
              </div>
            ) : (
              <EmptyState
                title="Nenhum evento compatível ainda"
                description="Registre seus interesses no perfil pra receber sugestões."
                action={
                  <Link to="/profile">
                    <Button variant="secondary" className="px-5 py-2.5">
                      Ir para o perfil
                    </Button>
                  </Link>
                }
              />
            )}
          </div>
        </section>
      </div>

      <section className="rounded-xl border border-line/70 p-6">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h2 className="font-display text-xl font-semibold text-ink">Passeio Surpresa</h2>
            <p className="mt-1 text-sm text-ink-soft">Deixe o app escolher um evento pra você.</p>
          </div>
          <Button
            variant="primary"
            className="gap-2 px-5 py-2.5"
            isLoading={randomOutingMutation.isPending}
            onClick={() => randomOutingMutation.mutate(undefined)}
          >
            <Shuffle size={18} />
            Sortear
          </Button>
        </div>
        {randomOutingMutation.data ? (
          <div className="mt-4 rounded-lg bg-paper-dim px-4 py-3 text-sm text-ink">
            {randomOutingMutation.data.suggestion ? (
              <p>
                <span className="font-medium">{randomOutingMutation.data.suggestion.title}</span>
                {randomOutingMutation.data.suggestion.cityName
                  ? ` em ${randomOutingMutation.data.suggestion.cityName}`
                  : ""}
              </p>
            ) : (
              <p>{randomOutingMutation.data.message}</p>
            )}
          </div>
        ) : null}
        {randomOutingMutation.isError ? (
          <div className="mt-4">
            <ErrorState onRetry={() => randomOutingMutation.mutate(undefined)} />
          </div>
        ) : null}
      </section>
    </div>
  );
}
