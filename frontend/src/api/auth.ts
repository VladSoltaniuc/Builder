// API layer — authentication
import { httpCore } from "./httpCore";
import type { AuthResult, LoginResult, Profile } from "../types/auth";

const RESOURCE = "/auth";

export const authApi = {
  register: (name: string, email: string, password: string) =>
    httpCore.post<AuthResult>(`${RESOURCE}/register`, { name, email, password }),

  login: (email: string, password: string) =>
    httpCore.post<LoginResult>(`${RESOURCE}/login`, { email, password }),

  verifyTwoFactor: (twoFactorToken: string, code: string) =>
    httpCore.post<AuthResult>(`${RESOURCE}/2fa/verify`, { twoFactorToken, code }),

  me: () => httpCore.get<Profile>(`${RESOURCE}/me`),
};
