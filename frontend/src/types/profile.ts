export type ProfileType = "Student" | "Tourist" | "Business";

export interface TravelerProfileResponse {
  profileType: ProfileType;
}

export interface UpdateProfileRequest {
  profileType: ProfileType;
}
