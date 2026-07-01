// Presentation layer - detail view
import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { User, UserInput } from "../types/user";
import type { Profile, Role } from "../types/auth";
import { ReportPreferenceFields } from "./ReportPreferenceFields";

// Bitmask values must match backend UserFeature enum
const FEATURES = [
  { bit: 1, key: "canExportExcel" },
  { bit: 2, key: "canViewAuditLog" },
  { bit: 4, key: "canManageInvoices" },
] as const;

interface UserFormProps {
  user: User | null;
  currentUser: Profile;
  onSubmit: (input: UserInput) => Promise<void>;
  onCancel: () => void;
}

const EMPTY_FORM: UserInput = {
  name: "",
  email: "",
  phoneNumber: "",
  reportChannel: "None",
  role: "Operator",
  features: 0,
};

export function UserForm({
  user,
  currentUser,
  onSubmit,
  onCancel,
}: Readonly<UserFormProps>) {
  const { t } = useTranslation();
  const [form, setForm] = useState<UserInput>(EMPTY_FORM);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const isSelf = user?.id === currentUser.id;
  const isAdmin = currentUser.role === "Admin";

  useEffect(() => {
    if (user) {
      setForm({
        name: user.name,
        email: user.email,
        phoneNumber: user.phoneNumber ?? "",
        reportChannel: user.reportChannel,
        role: user.role,
        features: user.features,
      });
    } else {
      setForm(EMPTY_FORM);
    }
  }, [user]);

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  }

  function handleRoleChange(role: Role) {
    setForm((prev) => ({
      ...prev,
      role,
      // Reset features whenever the role changes - Admins don't need them,
      // and it avoids carrying over stale bits when demoting to Operator
      features: 0,
    }));
  }

  function toggleFeature(bit: number) {
    setForm((prev) => ({ ...prev, features: prev.features ^ bit }));
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    try {
      await onSubmit(form);
    } finally {
      setIsSubmitting(false);
    }
  }

  const formTitle = user
    ? t("users.form.editTitle", { id: user.id })
    : t("users.form.addTitle");
  const submitLabel = user ? t("form.save") : t("users.add");
  const featuresDisabled = isSelf || form.role === "Admin";

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>{formTitle}</h2>

      {isSelf && <p className="error">{t("users.form.selfEditDisabled")}</p>}

      <div className="form-grid">
        <label>
          {t("users.form.name")}
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            required
            minLength={2}
            placeholder=" "
            disabled={isSelf}
          />
        </label>

        <label>
          {t("users.form.email")}
          <input
            name="email"
            type="email"
            value={form.email}
            onChange={handleChange}
            required
            placeholder=" "
            disabled={isSelf}
          />
        </label>

        {isAdmin && (
          <label>
            {t("users.form.role")}
            <select
              value={form.role}
              onChange={(e) => handleRoleChange(e.target.value as Role)}
              disabled={isSelf}
            >
              <option value="Operator">Operator</option>
              <option value="Admin">Admin</option>
            </select>
          </label>
        )}

        {isAdmin && (
          <fieldset
            disabled={featuresDisabled}
            style={{ border: "none", padding: 0, margin: 0 }}
          >
            <legend style={{ fontWeight: 500, marginBottom: 6 }}>
              {t("users.form.features")}
              {form.role === "Admin" && (
                <span
                  style={{
                    fontWeight: 400,
                    marginLeft: 8,
                    opacity: 0.6,
                    fontSize: "0.85em",
                  }}
                >
                  {t("users.form.featuresHint")}
                </span>
              )}
            </legend>
            {FEATURES.map(({ bit, key }) => (
              <label
                key={bit}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 8,
                  marginBottom: 4,
                  cursor: featuresDisabled ? "not-allowed" : "pointer",
                }}
              >
                <input
                  type="checkbox"
                  checked={form.role === "Admin" || (form.features & bit) !== 0}
                  onChange={() => toggleFeature(bit)}
                  disabled={featuresDisabled}
                />
                {t(`users.form.${key}`)}
              </label>
            ))}
          </fieldset>
        )}

        <ReportPreferenceFields
          channel={form.reportChannel}
          phoneNumber={form.phoneNumber ?? ""}
          onChannelChange={(reportChannel) =>
            setForm((prev) => ({ ...prev, reportChannel }))
          }
          onPhoneChange={(phoneNumber) =>
            setForm((prev) => ({ ...prev, phoneNumber }))
          }
        />
      </div>

      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting || isSelf}
        >
          {isSubmitting ? t("form.saving") : submitLabel}
        </button>
        {user && (
          <button
            type="button"
            className="btn"
            onClick={onCancel}
            disabled={isSubmitting}
          >
            {t("form.cancel")}
          </button>
        )}
      </div>
    </form>
  );
}
