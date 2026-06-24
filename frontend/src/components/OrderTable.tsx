// Presentation layer — list view
import type { Order } from "../types/order";

interface OrderTableProps {
  orders: Order[];
  onEdit: (order: Order) => void;
  onDelete: (id: number) => void;
}

const currency = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
});

const STATUS_LABELS: Record<string, string> = {
  Pending: "Pending",
  Completed: "Completed",
  Cancelled: "Cancelled",
};

export function OrderTable({ orders, onEdit, onDelete }: OrderTableProps) {
  if (orders.length === 0) {
    return <p className="empty">No orders. Add one using the form above.</p>;
  }

  return (
    <table className="table">
      <thead>
        <tr>
          <th>#</th>
          <th>User</th>
          <th>Product</th>
          <th className="num">Qty</th>
          <th className="num">Total</th>
          <th>Status</th>
          <th>Date</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order) => (
          <tr key={order.id}>
            <td>{order.id}</td>
            <td>{order.userName}</td>
            <td>{order.productName}</td>
            <td className="num">{order.quantity}</td>
            <td className="num">{currency.format(order.totalPrice)}</td>
            <td>{STATUS_LABELS[order.status] ?? order.status}</td>
            <td>{new Date(order.createdAt).toLocaleDateString()}</td>
            <td>
              <div className="row-actions">
                <button className="btn btn-small" onClick={() => onEdit(order)}>
                  Edit
                </button>
                <button
                  className="btn btn-small btn-danger"
                  onClick={() => onDelete(order.id)}
                >
                  Delete
                </button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
