import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getTravelerProfile, updateTravelerProfile } from "./api";

const profileKey = ["profile"] as const;

export function useTravelerProfile() {
  return useQuery({ queryKey: profileKey, queryFn: getTravelerProfile });
}

export function useUpdateTravelerProfile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: updateTravelerProfile,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: profileKey });
    },
  });
}
