import { Shuffle } from "@phosphor-icons/react";
import { Button } from "../components/Button";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { ErrorState } from "../components/ErrorState";
import { EmptyState } from "../components/EmptyState";
import { useDiscoveryFeed, useRandomOutingMutation } from "../features/discovery/hooks";
import { timeHorizonLabels } from "../features/discovery/labels";
import { interestCategoryLabels } from "../features/interests/labels";
import type { DiscoverySuggestion } from "../types/discovery";

export function DiscoveryFeedPage() {
  const feedQuery = useDiscoveryFeed();
  const randomOutingMutation = useRandomOutingMutation();

  return (
    <div className="flex flex-col gap-8">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="font-display text-3xl font-semibold text-ink">Descoberta</h1>
          <p className="mt-1 text-sm text-ink-soft">
            Eventos que combinam com seus interesses, por horizonte de tempo.
          </p>
        </div>
        <Button
          variant="secondary"
          className="gap-2 px-5 py-2.5"
          isLoading={randomOutingMutation.isPending}
          onClick={() => randomOutingMutation.mutate(undefined)}
        >
          <Shuffle size={18} />
          Passeio Surpresa
        </Button>
      </div>

      {randomOutingMutation.data ? (
        <div className="rounded-lg bg-paper-dim px-4 py-3 text-sm text-ink">
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

      {feedQuery.isLoading ? (
        <LoadingSpinner label="Carregando eventos" />
      ) : feedQuery.isError ? (
        <ErrorState onRetry={() => feedQuery.refetch()} />
      ) : (
        <div className="flex flex-col gap-8">
          {feedQuery.data?.groups.map((group) => (
            <section key={group.horizon}>
              <h2 className="font-display text-xl font-semibold text-ink">
                {timeHorizonLabels[group.horizon]}
              </h2>
              <div className="mt-4">
                {group.suggestions.length > 0 ? (
                  <ul className="flex flex-col gap-3">
                    {group.suggestions.map((suggestion) => (
                      <SuggestionRow key={suggestion.eventId} suggestion={suggestion} />
                    ))}
                  </ul>
                ) : (
                  <EmptyState title="Nenhum evento compatível nesse horizonte" />
                )}
              </div>
            </section>
          ))}
        </div>
      )}
    </div>
  );
}

function SuggestionRow({ suggestion }: { suggestion: DiscoverySuggestion }) {
  const startDate = new Date(suggestion.startUtc).toLocaleDateString("pt-BR");

  return (
    <li className="rounded-lg border border-line/70 px-4 py-3 text-sm">
      <div className="flex items-center justify-between gap-3">
        <p className="font-medium text-ink">{suggestion.title}</p>
        <span className="shrink-0 text-ink-soft">{startDate}</span>
      </div>
      <p className="mt-1 text-ink-soft">
        {interestCategoryLabels[suggestion.category]}
        {suggestion.cityName ? ` · ${suggestion.cityName}` : ""}
        {suggestion.venueName ? ` · ${suggestion.venueName}` : ""}
      </p>
    </li>
  );
}
