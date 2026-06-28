// Shared layer — auth contract types
export type Role = "Admin" | "Operator";
export type ReportChannel = "None" | "Email" | "Sms";

// GET /api/auth/me
export interface Profile {
  id: number;
  name: string;
  email: string;
  role: Role;
  features: number;
  phoneNumber: string | null;
  reportChannel: ReportChannel;
}

// The user payload embedded in an AuthResponse.
export interface AuthUser {
  id: number;
  name: string;
  email: string;
  phoneNumber: string | null;
  reportChannel: ReportChannel;
  version: number;
}

// POST /api/auth/register — no token; the user must verify their email first.
export interface RegisterResult {
  email: string;
  message: string;
}

// POST /api/auth/2fa/verify
export interface AuthResult {
  token: string;
  expiresAt: string;
  user: AuthUser;
}

// POST /api/auth/login — may demand a second factor instead of a token.
export interface LoginResult {
  requiresTwoFactor: boolean;
  twoFactorToken: string | null;
  auth: AuthResult | null;
}
