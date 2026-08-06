import { http, HttpResponse } from "msw";

const baseUrl = import.meta.env.VITE_API_BASE_URL;

interface RegisterBody {
  email: string;
  password: string;
  displayName: string;
}

interface LoginBody {
  email: string;
  password: string;
}

export const handlers = [
  http.post(`${baseUrl}/auth/register`, async ({ request }) => {
    const body = (await request.json()) as RegisterBody;
    return HttpResponse.json({
      token: "fake-token",
      expiresAtUtc: new Date(Date.now() + 7_200_000).toISOString(),
      email: body.email,
      displayName: body.displayName,
      role: "User",
    });
  }),

  http.post(`${baseUrl}/auth/login`, async ({ request }) => {
    const body = (await request.json()) as LoginBody;
    if (body.password !== "Senha123!") {
      return HttpResponse.json({ title: "Unauthorized", status: 401 }, { status: 401 });
    }
    return HttpResponse.json({
      token: "fake-token",
      expiresAtUtc: new Date(Date.now() + 7_200_000).toISOString(),
      email: body.email,
      displayName: "Ana Viajante",
      role: "User",
    });
  }),
];
