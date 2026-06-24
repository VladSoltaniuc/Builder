// Presentation layer — detail view
import { useEffect, useState, type FormEvent } from "react";
import type { User, UserInput } from "../types/user";

interface UserFormProps {
  user: User | null;
  onSubmit: (input: UserInput) => Promise<void>;
  onCancel: () => void;
}

const EMPTY_FORM: UserInput = { name: "", email: "" };

export function UserForm({ user, onSubmit, onCancel }: UserFormProps) {
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

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>{user ? `Edit User #${user.id}` : "Add User"}</h2>

      <div className="form-grid">
        <label>
          Name
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            required
            minLength={2}
          />
        </label>

        <label>
          Email
          <input
            name="email"
            type="email"
            value={form.email}
            onChange={handleChange}
            required
          />
        </label>
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : user ? "Save" : "Add User"}
        </button>
        {user && (
          <button type="button" className="btn" onClick={onCancel} disabled={isSubmitting}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
