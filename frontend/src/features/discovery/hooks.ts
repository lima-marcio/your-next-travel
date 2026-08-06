import { useMutation, useQuery } from "@tanstack/react-query";
import { getDiscoveryFeed, getRandomOuting } from "./api";
import type { TimeHorizon } from "../../types/discovery";

export function useDiscoveryFeed() {
  return useQuery({ queryKey: ["discovery", "feed"], queryFn: getDiscoveryFeed });
}

export function useRandomOutingMutation() {
  return useMutation({
    mutationFn: (horizon?: TimeHorizon) => getRandomOuting(horizon),
  });
}
