import type { ProfileType } from "../../types/profile";

export const profileTypeLabels: Record<ProfileType, string> = {
  Student: "Estudante",
  Tourist: "Turista",
  Business: "Negócios",
};

export const profileTypeOrder: ProfileType[] = ["Student", "Tourist", "Business"];
