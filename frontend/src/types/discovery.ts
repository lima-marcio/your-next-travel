import type { InterestCategory } from "./interests";

export type TimeHorizon = "Within1Week" | "NextMonth" | "NextSemester";

export interface DiscoverySuggestion {
  eventId: string;
  category: InterestCategory;
  title: string;
  cityName: string | null;
  venueName: string | null;
  startUtc: string;
  endUtc: string | null;
  externalUrl: string | null;
}

export interface DiscoveryFeedGroup {
  horizon: TimeHorizon;
  suggestions: DiscoverySuggestion[];
}

export interface DiscoveryFeedResponse {
  groups: DiscoveryFeedGroup[];
}

export interface RandomOutingResponse {
  suggestion: DiscoverySuggestion | null;
  message: string;
}
