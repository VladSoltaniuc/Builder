// API layer - report preferences
import { httpCore } from "./httpCore";
import type { ReportChannel } from "../types/auth";

const RESOURCE = "/reports";

export const reportsApi = {
  // Set the signed-in user's weekly report delivery preference
  setSubscription: (channel: ReportChannel, phoneNumber: string | null) =>
    httpCore.put<void>(`${RESOURCE}/subscription`, { channel, phoneNumber }),
};
