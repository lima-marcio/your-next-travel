import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getDestinationGuideBySearchId,
  getDestinationGuideHistory,
  searchDestinationGuide,
} from "./api";
import type { DestinationGuideSearchRequest } from "../../types/destinationGuide";

export const destinationGuideKeys = {
  history: ["destination-guide", "history"] as const,
  detail: (searchId: string) => ["destination-guide", "detail", searchId] as const,
};

export function useDestinationGuideHistory() {
  return useQuery({
    queryKey: destinationGuideKeys.history,
    queryFn: getDestinationGuideHistory,
  });
}

export function useDestinationGuideDetail(searchId: string | undefined) {
  return useQuery({
    queryKey: destinationGuideKeys.detail(searchId ?? ""),
    queryFn: () => getDestinationGuideBySearchId(searchId as string),
    enabled: Boolean(searchId),
  });
}

export function useDestinationGuideSearch() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: DestinationGuideSearchRequest) => searchDestinationGuide(request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: destinationGuideKeys.history });
    },
  });
}
