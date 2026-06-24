// Presentation layer — detail view
import { useEffect, useState, type FormEvent } from "react";
import type { Order, OrderInput, OrderUpdateInput } from "../types/order";

interface OrderFormProps {
  order: Order | null;
  onSubmit: (input: OrderInput | OrderUpdateInput) => Promise<void>;
  onCancel: () => void;
}

const STATUSES = ["Pending", "Completed", "Cancelled"];

export function OrderForm({ order, onSubmit, onCancel }: OrderFormProps) {
  const [userId, setUserId] = useState("");
  const [productId, setProductId] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [status, setStatus] = useState("Pending");
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (order) {
      setUserId(String(order.userId));
      setProductId(String(order.productId));
      setQuantity(order.quantity);
      setStatus(order.status);
    } else {
      setUserId("");
      setProductId("");
      setQuantity(1);
      setStatus("Pending");
    }
  }, [order]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    try {
      if (order) {
        const input: OrderUpdateInput = { quantity, status, version: order.version };
        await onSubmit(input);
      } else {
        const input: OrderInput = { userId: Number(userId), productId: Number(productId), quantity };
        await onSubmit(input);
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>{order ? `Edit Order #${order.id}` : "Create Order"}</h2>

      <div className="form-grid">
        {!order && (
          <>
            <label>
              User ID
              <input
                type="number"
                min="1"
                value={userId}
                onChange={(e) => setUserId(e.target.value)}
                required
              />
            </label>

            <label>
              Product ID
              <input
                type="number"
                min="1"
                value={productId}
                onChange={(e) => setProductId(e.target.value)}
                required
              />
            </label>
          </>
        )}

        <label>
          Quantity
          <input
            type="number"
            min="1"
            value={quantity}
            onChange={(e) => setQuantity(Number(e.target.value))}
            required
          />
        </label>

        {order && (
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              {STATUSES.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </label>
        )}
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : order ? "Save" : "Create Order"}
        </button>
        {order && (
          <button type="button" className="btn" onClick={onCancel} disabled={isSubmitting}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
