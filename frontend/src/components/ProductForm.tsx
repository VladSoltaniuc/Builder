// DetailView - Presentation layer
import { useEffect, useState, type FormEvent } from "react";
import type { Product, ProductInput } from "../types/product";

interface ProductFormProps {
  /** Dacă e dat, formularul e în mod "editare"; altfel, "creare". */
  editing: Product | null;
  onSubmit: (input: ProductInput) => Promise<void>;
  onCancel: () => void;
}

// Starea inițială goală pentru modul "creare".
const EMPTY_FORM: ProductInput = { name: "", category: "", price: 0, stock: 0 };

/**
 * Formular controlat ("controlled component"): React deține valorile câmpurilor în state,
 * iar input-urile le reflectă. Așa avem mereu o singură sursă de adevăr.
 */
export function ProductForm({ editing, onSubmit, onCancel }: ProductFormProps) {
  const [form, setForm] = useState<ProductInput>(EMPTY_FORM);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Când selectăm un produs pentru editare, populăm câmpurile cu valorile lui.
  useEffect(() => {
    if (editing) {
      const { name, category, price, stock } = editing;
      setForm({ name, category, price, stock });
    } else {
      setForm(EMPTY_FORM);
    }
  }, [editing]);

  // Un singur handler pentru toate input-urile, pe baza atributului "name".
  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    const { name, value, type } = event.target;
    setForm((prev) => ({
      ...prev,
      // Câmpurile numerice le convertim din string în număr.
      [name]: type === "number" ? Number(value) : value,
    }));
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault(); // Oprește reîncărcarea paginii (comportamentul default al formularului).
    setIsSubmitting(true);
    try {
      await onSubmit(form);
      if (!editing) {
        setForm(EMPTY_FORM); // După creare resetăm formularul.
      }
    } finally {
      setIsSubmitting(false);
    }
  }

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
          />
        </label>

        <label>
          Categorie
          <input
            name="category"
            value={form.category}
            onChange={handleChange}
            required
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
          />
        </label>
      </div>

      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Se salvează..." : editing ? "Salvează" : "Adaugă"}
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
