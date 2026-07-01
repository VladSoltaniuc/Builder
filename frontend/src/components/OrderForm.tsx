// Presentation layer - detail view
import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { Order, OrderInput, OrderUpdateInput } from "../types/order";

interface OrderFormProps {
  order: Order | null;
  onSubmit: (input: OrderInput | OrderUpdateInput) => Promise<void>;
  onCancel: () => void;
}

const STATUSES = ["Pending", "Completed", "Cancelled"];

export function OrderForm({
  order,
  onSubmit,
  onCancel,
}: Readonly<OrderFormProps>) {
  const { t } = useTranslation();
  const [userId, setUserId] = useState("");
  const [productId, setProductId] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [status, setStatus] = useState("Pending");
  const [awb, setAwb] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (order) {
      setUserId(String(order.userId));
      setProductId(String(order.productId));
      setQuantity(order.quantity);
      setStatus(order.status);
      setAwb(order.awb ?? "");
    } else {
      setUserId("");
      setProductId("");
      setQuantity(1);
      setStatus("Pending");
      setAwb("");
    }
  }, [order]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    try {
      if (order) {
        await onSubmit({
          quantity,
          status,
          version: order.version,
          awb: awb || undefined,
        } as OrderUpdateInput);
      } else {
        await onSubmit({
          userId: Number(userId),
          productId: Number(productId),
          quantity,
        } as OrderInput);
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  const formTitle = order
    ? t("orders.form.editTitle", { id: order.id })
    : t("orders.form.addTitle");
  const submitLabel = order ? t("form.save") : t("orders.form.addTitle");

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>{formTitle}</h2>

      <div className="form-grid">
        {!order && (
          <>
            <label>
              {t("orders.form.userId")}
              <input
                type="number"
                min="1"
                value={userId}
                onChange={(e) => setUserId(e.target.value)}
                required
                placeholder=" "
              />
            </label>

            <label>
              {t("orders.form.productId")}
              <input
                type="number"
                min="1"
                value={productId}
                onChange={(e) => setProductId(e.target.value)}
                required
                placeholder=" "
              />
            </label>
          </>
        )}

        <label>
          {t("orders.form.quantity")}
          <input
            type="number"
            min="1"
            value={quantity}
            onChange={(e) => setQuantity(Number(e.target.value))}
            required
            placeholder=" "
          />
        </label>

        {order && (
          <>
            <label>
              {t("orders.form.status")}
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
              >
                {STATUSES.map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
              </select>
            </label>
            <label>
              {t("orders.form.awb")}
              <input
                type="text"
                value={awb}
                onChange={(e) => setAwb(e.target.value)}
                maxLength={50}
                placeholder=" "
              />
            </label>
          </>
        )}
      </div>

      <div className="form-actions">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting}
        >
          {isSubmitting ? t("form.saving") : submitLabel}
        </button>
        {order && (
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
