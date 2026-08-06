import { apiClient } from "../../api/client";
import type { DiscoveryFeedResponse, RandomOutingResponse, TimeHorizon } from "../../types/discovery";

export async function getDiscoveryFeed(): Promise<DiscoveryFeedResponse> {
  const { data } = await apiClient.get<DiscoveryFeedResponse>("/discovery/feed");
  return data;
}

export async function getRandomOuting(horizon?: TimeHorizon): Promise<RandomOutingResponse> {
  const { data } = await apiClient.get<RandomOutingResponse>("/discovery/random-outing", {
    params: horizon ? { horizon } : undefined,
  });
  return data;
}
