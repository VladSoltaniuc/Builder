// Application layer — self-service profile + report preferences
import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import toast from "react-hot-toast";
import { useAuth } from "../context/AuthContext";
import { reportsApi } from "../api/reports";
import { ReportPreferenceFields } from "../components/ReportPreferenceFields";
import { ApiError } from "../api/errors";
import type { ReportChannel } from "../types/auth";

export function ProfilePage() {
  const { t } = useTranslation();
  const { user, refresh } = useAuth();

  const [channel, setChannel] = useState<ReportChannel>("None");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Seed the form from the loaded profile.
  useEffect(() => {
    if (user) {
      setChannel(user.reportChannel);
      setPhoneNumber(user.phoneNumber ?? "");
    }
  }, [user]);

  if (!user) return null;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    try {
      await reportsApi.setSubscription(channel, phoneNumber.trim() || null);
      await refresh();
      toast.success(t("profile.saved"));
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : t("profile.saveFailed"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="container">
      <form className="card auth-card" onSubmit={handleSubmit}>
        <h1>{t("profile.title")}</h1>

        <div className="profile-info">
          <p><strong>{t("auth.name")}:</strong> {user.name}</p>
          <p><strong>{t("auth.email")}:</strong> {user.email}</p>
          <p><strong>{t("profile.role")}:</strong> {user.role}</p>
        </div>

        <h2>{t("profile.reportPrefsTitle")}</h2>
        <ReportPreferenceFields
          channel={channel}
          phoneNumber={phoneNumber}
          onChannelChange={setChannel}
          onPhoneChange={setPhoneNumber}
        />

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
            {isSubmitting ? t("form.saving") : t("form.save")}
          </button>
        </div>
      </form>
    </main>
  );
}
