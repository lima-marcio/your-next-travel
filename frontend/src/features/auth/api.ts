import { apiClient } from "../../api/client";
import type { AuthResponse, LoginRequest, RegisterRequest } from "../../types/auth";

export async function registerUser(request: RegisterRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>("/auth/register", request);
  return data;
}

export async function loginUser(request: LoginRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>("/auth/login", request);
  return data;
}
