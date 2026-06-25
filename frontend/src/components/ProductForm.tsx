// Presentation layer — detail view
import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { Product, ProductInput } from "../types/product";

interface ProductFormProps {
  product: Product | null;
  onSubmit: (input: ProductInput) => Promise<void>;
  onCancel: () => void;
}

const EMPTY_FORM: ProductInput = { name: "", category: "", price: 0, stock: 0 };

function validate(message: string) {
  return {
    onInvalid: (event: React.InvalidEvent<HTMLInputElement>) =>
      event.target.setCustomValidity(message),
    onInput: (event: React.FormEvent<HTMLInputElement>) =>
      (event.target as HTMLInputElement).setCustomValidity(""),
  };
}

export function ProductForm({ product, onSubmit, onCancel }: Readonly<ProductFormProps>) {
  const { t } = useTranslation();
  const [form, setForm] = useState<ProductInput>(EMPTY_FORM);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (product) {
      const { name, category, price, stock } = product;
      setForm({ name, category, price, stock });
    } else {
      setForm(EMPTY_FORM);
    }
  }, [product]);

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

  const submitLabel = product ? t('form.save') : t('form.add');

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>{product ? t('products.form.editTitle', { id: product.id }) : t('products.form.addTitle')}</h2>

      <div className="form-grid">
        <label>
          {t('products.form.name')}
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            required
            minLength={2}
            {...validate(t('products.validation.name'))}
          />
        </label>

        <label>
          {t('products.form.category')}
          <input
            name="category"
            value={form.category}
            onChange={handleChange}
            required
            {...validate(t('products.validation.category'))}
          />
        </label>

        <label>
          {t('products.form.price')}
          <input
            name="price"
            type="number"
            step="0.01"
            min="0.01"
            value={form.price}
            onChange={handleChange}
            required
            {...validate(t('products.validation.price'))}
          />
        </label>

        <label>
          {t('products.form.stock')}
          <input
            name="stock"
            type="number"
            min="0"
            value={form.stock}
            onChange={handleChange}
            required
            {...validate(t('products.validation.stock'))}
          />
        </label>
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? t('form.saving') : submitLabel}
        </button>
        {product && (
          <button type="button" className="btn" onClick={onCancel} disabled={isSubmitting}>
            {t('form.cancel')}
          </button>
        )}
      </div>
    </form>
  );
}
