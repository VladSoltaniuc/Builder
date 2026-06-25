// Presentation layer — detail view
import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { User, UserInput } from "../types/user";

interface UserFormProps {
  user: User | null;
  onSubmit: (input: UserInput) => Promise<void>;
  onCancel: () => void;
}

const EMPTY_FORM: UserInput = { name: "", email: "" };

export function UserForm({ user, onSubmit, onCancel }: Readonly<UserFormProps>) {
  const { t } = useTranslation();
  const [form, setForm] = useState<UserInput>(EMPTY_FORM);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (user) {
      setForm({ name: user.name, email: user.email });
    } else {
      setForm(EMPTY_FORM);
    }
  }, [user]);

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
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

  const formTitle = user ? t('users.form.editTitle', { id: user.id }) : t('users.form.addTitle');
  const submitLabel = user ? t('form.save') : t('users.add');

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>{formTitle}</h2>

      <div className="form-grid">
        <label>
          {t('users.form.name')}
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            required
            minLength={2}
            placeholder=" "
          />
        </label>

        <label>
          {t('users.form.email')}
          <input
            name="email"
            type="email"
            value={form.email}
            onChange={handleChange}
            required
            placeholder=" "
          />
        </label>
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? t('form.saving') : submitLabel}
        </button>
        {user && (
          <button type="button" className="btn" onClick={onCancel} disabled={isSubmitting}>
            {t('form.cancel')}
          </button>
        )}
      </div>
    </form>
  );
}
