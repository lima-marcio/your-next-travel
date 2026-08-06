import { apiClient } from "../../api/client";
import type { CreateInterestRequest, InterestResponse } from "../../types/interests";

export async function listInterests(): Promise<InterestResponse[]> {
  const { data } = await apiClient.get<InterestResponse[]>("/interests");
  return data;
}

export async function addInterest(request: CreateInterestRequest): Promise<InterestResponse> {
  const { data } = await apiClient.post<InterestResponse>("/interests", request);
  return data;
}

export async function removeInterest(interestId: string): Promise<void> {
  await apiClient.delete(`/interests/${interestId}`);
}
