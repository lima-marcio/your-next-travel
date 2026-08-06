import type { InterestCategory } from "../../types/interests";

export const interestCategoryLabels: Record<InterestCategory, string> = {
  MotorsportF1: "Automobilismo (F1)",
  MotorsportF2: "Automobilismo (F2)",
  MotorsportDtm: "Automobilismo (DTM)",
  MotorsportStockCar: "Stock Car",
  Football: "Futebol",
  Auctions: "Leilões",
  ConcertsShows: "Shows e concertos",
  CulturalFestivals: "Festivais culturais",
};

export const interestCategoryOrder: InterestCategory[] = [
  "MotorsportF1",
  "MotorsportF2",
  "MotorsportDtm",
  "MotorsportStockCar",
  "Football",
  "Auctions",
  "ConcertsShows",
  "CulturalFestivals",
];
