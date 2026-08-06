import { useMutation } from "@tanstack/react-query";
import { loginUser, registerUser } from "./api";
import type { LoginRequest, RegisterRequest } from "../../types/auth";

export function useRegisterMutation() {
  return useMutation({
    mutationFn: (request: RegisterRequest) => registerUser(request),
  });
}

export function useLoginMutation() {
  return useMutation({
    mutationFn: (request: LoginRequest) => loginUser(request),
  });
}
