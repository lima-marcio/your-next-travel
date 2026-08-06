import { CheckCircle } from "@phosphor-icons/react";
import { useAddInterest, useInterests, useRemoveInterest } from "./hooks";
import { interestCategoryLabels, interestCategoryOrder } from "./labels";
import { LoadingSpinner } from "../../components/LoadingSpinner";
import { ErrorState } from "../../components/ErrorState";
import type { InterestCategory } from "../../types/interests";

export function InterestsSection() {
  const interestsQuery = useInterests();
  const addInterest = useAddInterest();
  const removeInterest = useRemoveInterest();

  if (interestsQuery.isLoading) {
    return <LoadingSpinner label="Carregando interesses" />;
  }

  if (interestsQuery.isError) {
    return <ErrorState onRetry={() => interestsQuery.refetch()} />;
  }

  const interests = interestsQuery.data ?? [];

  function toggle(category: InterestCategory) {
    const existing = interests.find((interest) => interest.category === category);
    if (existing) {
      removeInterest.mutate(existing.id);
    } else {
      addInterest.mutate(category);
    }
  }

  return (
    <div className="flex flex-wrap gap-2">
      {interestCategoryOrder.map((category) => {
        const active = interests.some((interest) => interest.category === category);
        return (
          <button
            key={category}
            type="button"
            onClick={() => toggle(category)}
            disabled={addInterest.isPending || removeInterest.isPending}
            className={`inline-flex items-center gap-1.5 rounded-full border px-4 py-2 text-sm font-medium transition-colors disabled:opacity-60 ${
              active
                ? "border-brand bg-brand-soft text-brand-strong"
                : "border-line text-ink-soft hover:border-ink/30 hover:text-ink"
            }`}
          >
            {active ? <CheckCircle size={16} weight="fill" /> : null}
            {interestCategoryLabels[category]}
          </button>
        );
      })}
    </div>
  );
}
