// Presentation layer — detail view
import { useEffect, useState, type FormEvent } from "react";
import type { Product, ProductInput } from "../types/product";
import {
  validationMessages,
  formLabels,
} from "../constants/validationMessages";

interface ProductFormProps {
  editing: Product | null;
  onSubmit: (input: ProductInput) => Promise<void>;
  onCancel: () => void;
}

const EMPTY_FORM: ProductInput = { name: "", category: "", price: 0, stock: 0 };

export function ProductForm({ editing, onSubmit, onCancel }: ProductFormProps) {
  const [form, setForm] = useState<ProductInput>(EMPTY_FORM);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (editing) {
      const { name, category, price, stock } = editing;
      setForm({ name, category, price, stock });
    } else {
      setForm(EMPTY_FORM);
    }
  }, [editing]);

  function validate(message: string) {
    return {
      onInvalid: (e: React.InvalidEvent<HTMLInputElement>) =>
        e.target.setCustomValidity(message),
      onInput: (e: React.FormEvent<HTMLInputElement>) =>
        (e.target as HTMLInputElement).setCustomValidity(""),
    };
  }

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    const { name, value, type } = event.target;
    setForm((prev) => ({
      ...prev,
      [name]: type === "number" ? Number(value) : value,
    }));
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

  const submitLabel = editing ? formLabels.save : formLabels.add;

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>{editing ? `Editează produsul #${editing.id}` : "Adaugă produs"}</h2>

      <div className="form-grid">
        <label>
          Nume
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            required
            minLength={2}
            {...validate(validationMessages.name)}
          />
        </label>

        <label>
          Categorie
          <input
            name="category"
            value={form.category}
            onChange={handleChange}
            required
            {...validate(validationMessages.category)}
          />
        </label>

        <label>
          Preț (RON)
          <input
            name="price"
            type="number"
            step="0.01"
            min="0.01"
            value={form.price}
            onChange={handleChange}
            required
            {...validate(validationMessages.price)}
          />
        </label>

        <label>
          Stoc
          <input
            name="stock"
            type="number"
            min="0"
            value={form.stock}
            onChange={handleChange}
            required
            {...validate(validationMessages.stock)}
          />
        </label>
      </div>

      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting}
        >
          {isSubmitting ? formLabels.saving : submitLabel}
        </button>
        {editing && (
          <button
            type="button"
            className="btn"
            onClick={onCancel}
            disabled={isSubmitting}
          >
            Anulează
          </button>
        )}
      </div>
    </form>
  );
}
