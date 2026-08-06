import { apiClient } from "../../api/client";
import type {
  DestinationGuideResponse,
  DestinationGuideSearchRequest,
  DestinationSearchSummary,
} from "../../types/destinationGuide";

export async function searchDestinationGuide(
  request: DestinationGuideSearchRequest,
): Promise<DestinationGuideResponse> {
  const { data } = await apiClient.post<DestinationGuideResponse>("/destination-guide/search", request);
  return data;
}

export async function getDestinationGuideHistory(): Promise<DestinationSearchSummary[]> {
  const { data } = await apiClient.get<DestinationSearchSummary[]>("/destination-guide/history");
  return data;
}

export async function getDestinationGuideBySearchId(searchId: string): Promise<DestinationGuideResponse> {
  const { data } = await apiClient.get<DestinationGuideResponse>(`/destination-guide/${searchId}`);
  return data;
}
