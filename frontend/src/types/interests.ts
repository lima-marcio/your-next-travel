export type InterestCategory =
  | "MotorsportF1"
  | "MotorsportF2"
  | "MotorsportDtm"
  | "MotorsportStockCar"
  | "Football"
  | "Auctions"
  | "ConcertsShows"
  | "CulturalFestivals";

export interface InterestResponse {
  id: string;
  category: InterestCategory;
  detail: string | null;
}

export interface CreateInterestRequest {
  category: InterestCategory;
  detail?: string;
}
