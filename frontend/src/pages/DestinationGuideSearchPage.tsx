import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { TextField } from "../components/TextField";
import { Button } from "../components/Button";
import { ErrorBanner } from "../components/ErrorBanner";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { ErrorState } from "../components/ErrorState";
import { EmptyState } from "../components/EmptyState";
import {
  useDestinationGuideHistory,
  useDestinationGuideSearch,
} from "../features/destinationGuide/hooks";
import {
  destinationGuideSearchSchema,
  type DestinationGuideSearchFormValues,
} from "../features/destinationGuide/schemas";
import { parseApiError } from "../api/parseApiError";

export function DestinationGuideSearchPage() {
  const navigate = useNavigate();
  const searchMutation = useDestinationGuideSearch();
  const historyQuery = useDestinationGuideHistory();
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<DestinationGuideSearchFormValues>({
    resolver: zodResolver(destinationGuideSearchSchema),
  });

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null);
    try {
      const result = await searchMutation.mutateAsync(values);
      navigate(`/guide/${result.searchId}`);
    } catch (error) {
      const parsed = parseApiError(error, "Não foi possível buscar esse destino. Tente novamente.");
      setFormError(parsed.message);
    }
  });

  return (
    <div className="flex flex-col gap-10">
      <div>
        <h1 className="font-display text-3xl font-semibold text-ink">Guia de destino</h1>
        <p className="mt-1 text-sm text-ink-soft">
          Clima, câmbio, hospedagem, exigências de saúde e orçamento estimado.
        </p>
      </div>

      <form
        onSubmit={onSubmit}
        noValidate
        className="flex flex-col gap-5 rounded-xl border border-line/70 p-6 sm:max-w-xl"
      >
        {formError ? <ErrorBanner message={formError} /> : null}
        <TextField
          label="Destino"
          placeholder="Ex.: Lisboa"
          error={errors.destination?.message}
          {...register("destination")}
        />
        <div className="grid gap-4 sm:grid-cols-2">
          <TextField
            label="Data de ida"
            type="date"
            error={errors.startDate?.message}
            {...register("startDate")}
          />
          <TextField
            label="Data de volta"
            type="date"
            error={errors.endDate?.message}
            {...register("endDate")}
          />
        </div>
        <Button type="submit" isLoading={isSubmitting} className="w-fit px-6 py-3">
          Buscar
        </Button>
      </form>

      <div>
        <h2 className="font-display text-xl font-semibold text-ink">Buscas recentes</h2>
        <div className="mt-4">
          {historyQuery.isLoading ? (
            <LoadingSpinner label="Carregando buscas" />
          ) : historyQuery.isError ? (
            <ErrorState onRetry={() => historyQuery.refetch()} />
          ) : historyQuery.data && historyQuery.data.length > 0 ? (
            <ul className="divide-y divide-line/70 rounded-xl border border-line/70">
              {historyQuery.data.map((search) => (
                <li key={search.searchId}>
                  <Link
                    to={`/guide/${search.searchId}`}
                    className="flex items-center justify-between px-5 py-4 text-sm hover:bg-paper-dim"
                  >
                    <span className="font-medium text-ink">{search.cityName}</span>
                    <span className="text-ink-soft">
                      {search.startDate} – {search.endDate}
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          ) : (
            <EmptyState
              title="Nenhuma busca ainda"
              description="Suas buscas de destino vão aparecer aqui."
            />
          )}
        </div>
      </div>
    </div>
  );
}
