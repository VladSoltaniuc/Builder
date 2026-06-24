// Shared layer — API contract types
export interface User {
  id: number;
  name: string;
  email: string;
  version: number;
}

export type UserInput = Omit<User, 'id' | 'version'>;
