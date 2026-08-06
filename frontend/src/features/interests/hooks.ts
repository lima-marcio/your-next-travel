import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { addInterest, listInterests, removeInterest } from "./api";
import type { InterestCategory } from "../../types/interests";

const interestsKey = ["interests"] as const;

export function useInterests() {
  return useQuery({ queryKey: interestsKey, queryFn: listInterests });
}

export function useAddInterest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (category: InterestCategory) => addInterest({ category }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: interestsKey });
    },
  });
}

export function useRemoveInterest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (interestId: string) => removeInterest(interestId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: interestsKey });
    },
  });
}
