// Shared layer — API contract types
import type { ReportChannel } from "./auth";

export interface User {
  id: number;
  name: string;
  email: string;
  phoneNumber: string | null;
  reportChannel: ReportChannel;
  version: number;
}

export type UserInput = Omit<User, 'id' | 'version'>;
