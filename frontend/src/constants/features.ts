// Shared layer - user feature flags
// Bitmask values must match backend UserFeature enum (Models/User.cs) and UserForm
import type { Profile } from "../types/auth";

export const UserFeature = {
  CanExportExcel: 1,
  CanViewAuditLog: 2,
  CanManageInvoices: 4,
} as const;

// Admins can do everything; operators need the specific feature bit granted
export function hasFeature(user: Profile | null, feature: number): boolean {
  if (!user) return false;
  return user.role === "Admin" || (user.features & feature) !== 0;
}
