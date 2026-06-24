// Application layer
import { useState } from "react";
import { createPortal } from "react-dom";
import { useOrders } from "../hooks/useOrders";
import { OrderForm } from "../components/OrderForm";
import { OrderTable } from "../components/OrderTable";
import type { Order, OrderInput, OrderUpdateInput } from "../types/order";

export function OrdersPage() {
  const {
    orders,
    isLoading,
    error,
    page,
    totalPages,
    setPage,
    createOrder,
    updateOrder,
    deleteOrder,
  } = useOrders();

  const [order, setEditing] = useState<Order | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  function openCreate() {
    setIsModalOpen(true);
    setEditing(null);
  }

  function openEdit(o: Order) {
    setIsModalOpen(true);
    setEditing(o);
  }

  function closeModal() {
    setIsModalOpen(false);
    setEditing(null);
  }

  async function handleSubmit(input: OrderInput | OrderUpdateInput) {
    if (order) {
      await updateOrder(order.id, input as OrderUpdateInput);
    } else {
      await createOrder(input as OrderInput);
    }
    closeModal();
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Delete this order?")) return;
    if (order?.id === id) closeModal();
    await deleteOrder(id);
  }

  return (
    <main className="container">
      <header>
        <h1>Orders</h1>
      </header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="toolbar">
        <button className="btn btn-primary" onClick={openCreate}>
          + Create Order
        </button>
      </div>

      {isLoading ? (
        <p className="loading">Loading...</p>
      ) : (
        <OrderTable orders={orders} onEdit={openEdit} onDelete={handleDelete} />
      )}

      <div className="pagination">
        <button className="btn" disabled={page === 1} onClick={() => setPage(page - 1)}>
          Previous
        </button>
        <span>Page {page} of {totalPages}</span>
        <button className="btn" disabled={page === totalPages} onClick={() => setPage(page + 1)}>
          Next
        </button>
      </div>

      {isModalOpen &&
        createPortal(
          <div className="modal-backdrop" onClick={closeModal}>
            <div
              className="modal"
              role="dialog"
              aria-modal="true"
              onClick={(e) => e.stopPropagation()}
              onKeyDown={(e) => e.stopPropagation()}
            >
              <OrderForm order={order} onSubmit={handleSubmit} onCancel={closeModal} />
            </div>
          </div>,
          document.body,
        )}
    </main>
  );
}
