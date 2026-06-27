// Presentation layer — shared report-delivery preference inputs
import { useTranslation } from "react-i18next";
import type { ReportChannel } from "../types/auth";

interface ReportPreferenceFieldsProps {
  channel: ReportChannel;
  phoneNumber: string;
  onChannelChange: (channel: ReportChannel) => void;
  onPhoneChange: (phone: string) => void;
}

// Channel dropdown (None/Email/SMS) plus a phone field shown only when SMS is the
// chosen channel — with a country-code hint so users include the prefix.
export function ReportPreferenceFields({
  channel,
  phoneNumber,
  onChannelChange,
  onPhoneChange,
}: Readonly<ReportPreferenceFieldsProps>) {
  const { t } = useTranslation();

  return (
    <>
      <label>
        {t("report.channel")}
        <select value={channel} onChange={(e) => onChannelChange(e.target.value as ReportChannel)}>
          <option value="None">{t("report.channelNone")}</option>
          <option value="Email">{t("report.channelEmail")}</option>
          <option value="Sms">{t("report.channelSms")}</option>
        </select>
      </label>

      {channel === "Sms" && (
        <label>
          {t("report.phone")}
          <input
            name="phoneNumber"
            type="tel"
            value={phoneNumber}
            onChange={(e) => onPhoneChange(e.target.value)}
            placeholder="+40712345678"
            required
          />
          <small className="field-hint">{t("report.phoneHint")}</small>
        </label>
      )}
    </>
  );
}
