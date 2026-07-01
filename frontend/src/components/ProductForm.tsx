// Presentation layer - detail view
import { useEffect, useRef, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { Product, ProductInput } from "../types/product";

interface ProductFormProps {
  product: Product | null;
  onSubmit: (input: ProductInput) => Promise<void>;
  onCancel: () => void;
  onUploadImage?: (file: File) => Promise<Product>;
  onDeleteImage?: () => Promise<void>;
}

const EMPTY_FORM: ProductInput = { name: "", category: "", price: 0, stock: 0 };

const STATIC_BASE = import.meta.env.VITE_STATIC_BASE_URL as string;

function validate(message: string) {
  return {
    onInvalid: (event: React.InvalidEvent<HTMLInputElement>) =>
      event.target.setCustomValidity(message),
    onInput: (event: React.FormEvent<HTMLInputElement>) =>
      (event.target as HTMLInputElement).setCustomValidity(""),
  };
}

export function ProductForm({
  product,
  onSubmit,
  onCancel,
  onUploadImage,
  onDeleteImage,
}: Readonly<ProductFormProps>) {
  const { t } = useTranslation();
  const [form, setForm] = useState<ProductInput>(EMPTY_FORM);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [imageUrl, setImageUrl] = useState<string | undefined>(
    product?.imageUrl,
  );
  const [isImageLoading, setIsImageLoading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (product) {
      const { name, category, price, stock } = product;
      setForm({ name, category, price, stock });
    } else {
      setForm(EMPTY_FORM);
    }
    setImageUrl(product?.imageUrl);
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

  async function handleFileChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (!file || !onUploadImage) return;
    setIsImageLoading(true);
    try {
      const updated = await onUploadImage(file);
      setImageUrl(updated.imageUrl);
    } finally {
      setIsImageLoading(false);
      event.target.value = "";
    }
  }

  async function handleDeleteImage() {
    if (!onDeleteImage) return;
    setIsImageLoading(true);
    try {
      await onDeleteImage();
      setImageUrl(undefined);
    } finally {
      setIsImageLoading(false);
    }
  }

  const submitLabel = product ? t("form.save") : t("form.add");

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>
        {product
          ? t("products.form.editTitle", { id: product.id })
          : t("products.form.addTitle")}
      </h2>

      <div className="form-grid">
        <label>
          {t("products.form.name")}
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            required
            minLength={2}
            {...validate(t("products.validation.name"))}
          />
        </label>

        <label>
          {t("products.form.category")}
          <input
            name="category"
            value={form.category}
            onChange={handleChange}
            required
            {...validate(t("products.validation.category"))}
          />
        </label>

        <label>
          {t("products.form.price")}
          <input
            name="price"
            type="number"
            step="0.01"
            min="0.01"
            value={form.price}
            onChange={handleChange}
            required
            {...validate(t("products.validation.price"))}
          />
        </label>

        <label>
          {t("products.form.stock")}
          <input
            name="stock"
            type="number"
            min="0"
            value={form.stock}
            onChange={handleChange}
            required
            {...validate(t("products.validation.stock"))}
          />
        </label>
      </div>

      {product && onUploadImage && (
        <div className="image-section">
          <p className="image-label">{t("products.form.image")}</p>
          {imageUrl && (
            <img
              src={`${STATIC_BASE}${imageUrl}`}
              alt={form.name}
              className="product-image-preview"
            />
          )}
          <div className="row-actions">
            <button
              type="button"
              className="btn btn-small"
              onClick={() => fileInputRef.current?.click()}
              disabled={isImageLoading}
            >
              {imageUrl
                ? t("products.form.changeImage")
                : t("products.form.uploadImage")}
            </button>
            {imageUrl && onDeleteImage && (
              <button
                type="button"
                className="btn btn-small btn-danger"
                onClick={handleDeleteImage}
                disabled={isImageLoading}
              >
                {t("products.form.removeImage")}
              </button>
            )}
          </div>
          <input
            ref={fileInputRef}
            type="file"
            accept=".jpg,.jpeg,.png,.webp"
            style={{ display: "none" }}
            onChange={handleFileChange}
          />
        </div>
      )}

      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting}
        >
          {isSubmitting ? t("form.saving") : submitLabel}
        </button>
        {product && (
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
