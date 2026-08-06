import type { ProfileType } from "./profile";
import type { InterestCategory } from "./interests";

export interface DestinationGuideSearchRequest {
  destination: string;
  startDate: string;
  endDate: string;
  profileTypeOverride?: ProfileType;
}

export interface WeatherSummary {
  available: boolean;
  isForecast: boolean;
  avgTempC: number | null;
  minTempC: number | null;
  maxTempC: number | null;
  precipitationMm: number | null;
}

export interface CurrencySummary {
  available: boolean;
  homeCurrencyCode: string;
  localCurrencyCode: string;
  homeToLocalRate: number | null;
  asOfDate: string | null;
}

export interface LodgingSummary {
  available: boolean;
  avgNightlyAmount: number | null;
  currency: string | null;
  minNightlyAmount: number | null;
  maxNightlyAmount: number | null;
  sampleWindowStart: string | null;
  sampleWindowEnd: string | null;
}

export interface LegalHealthSummary {
  available: boolean;
  visaRequirementText: string | null;
  vaccinationRequirementText: string | null;
  otherHealthNotes: string | null;
  sourceNote: string | null;
}

export interface BudgetSummary {
  lodgingComponentAmount: number;
  miscDailyComponentAmount: number;
  totalAmount: number;
  currency: string;
  assumptionsNote: string;
}

export interface MatchingEvent {
  id: string;
  category: InterestCategory;
  title: string;
  venueName: string | null;
  startUtc: string;
  endUtc: string | null;
  distanceKm: number | null;
  externalUrl: string | null;
}

export interface DestinationGuideResponse {
  searchId: string;
  cityName: string;
  countryName: string;
  startDate: string;
  endDate: string;
  profileTypeUsed: ProfileType;
  weather: WeatherSummary;
  currency: CurrencySummary;
  lodging: LodgingSummary;
  legalHealth: LegalHealthSummary;
  budget: BudgetSummary;
  matchingEvents: MatchingEvent[];
}

export interface DestinationSearchSummary {
  searchId: string;
  cityName: string;
  startDate: string;
  endDate: string;
  createdAtUtc: string;
}
