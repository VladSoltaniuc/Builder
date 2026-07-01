// Shared layer - API contract types
import type { ReportChannel, Role } from "./auth";

export interface User {
  id: number;
  name: string;
  email: string;
  phoneNumber: string | null;
  reportChannel: ReportChannel;
  role: Role;
  features: number;
  version: number;
}

export type UserInput = Omit<User, 'id' | 'version'>;
