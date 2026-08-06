import { apiClient } from "../../api/client";
import type { TravelerProfileResponse, UpdateProfileRequest } from "../../types/profile";

export async function getTravelerProfile(): Promise<TravelerProfileResponse> {
  const { data } = await apiClient.get<TravelerProfileResponse>("/profile");
  return data;
}

export async function updateTravelerProfile(
  request: UpdateProfileRequest,
): Promise<TravelerProfileResponse> {
  const { data } = await apiClient.put<TravelerProfileResponse>("/profile", request);
  return data;
}
