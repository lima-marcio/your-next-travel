import { useEffect, useState } from "react";
import { useTravelerProfile, useUpdateTravelerProfile } from "./hooks";
import { profileTypeLabels, profileTypeOrder } from "./labels";
import { LoadingSpinner } from "../../components/LoadingSpinner";
import { ErrorState } from "../../components/ErrorState";
import { Button } from "../../components/Button";
import type { ProfileType } from "../../types/profile";

export function TravelerProfileSection() {
  const profileQuery = useTravelerProfile();
  const updateProfile = useUpdateTravelerProfile();
  const [selected, setSelected] = useState<ProfileType | null>(null);

  useEffect(() => {
    if (profileQuery.data) {
      setSelected(profileQuery.data.profileType);
    }
  }, [profileQuery.data]);

  if (profileQuery.isLoading) {
    return <LoadingSpinner label="Carregando perfil de viagem" />;
  }

  if (profileQuery.isError) {
    return <ErrorState onRetry={() => profileQuery.refetch()} />;
  }

  const dirty = selected !== null && selected !== profileQuery.data?.profileType;

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap gap-3">
        {profileTypeOrder.map((type) => (
          <label
            key={type}
            className={`flex cursor-pointer items-center gap-2 rounded-full border px-4 py-2 text-sm font-medium transition-colors ${
              selected === type
                ? "border-brand bg-brand-soft text-brand-strong"
                : "border-line text-ink-soft hover:border-ink/30 hover:text-ink"
            }`}
          >
            <input
              type="radio"
              name="profileType"
              value={type}
              checked={selected === type}
              onChange={() => setSelected(type)}
              className="sr-only"
            />
            {profileTypeLabels[type]}
          </label>
        ))}
      </div>
      <Button
        type="button"
        variant="secondary"
        className="w-fit px-5 py-2.5"
        disabled={!dirty}
        isLoading={updateProfile.isPending}
        onClick={() => selected && updateProfile.mutate({ profileType: selected })}
      >
        Salvar
      </Button>
      {updateProfile.isSuccess ? <p className="text-sm text-brand-strong">Perfil atualizado.</p> : null}
    </div>
  );
}
