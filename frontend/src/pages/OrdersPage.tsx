// Application layer
import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import { useOrders } from "../hooks/useOrders";
import { ordersApi } from "../api/orders";
import { OrderForm } from "../components/OrderForm";
import { OrderTable } from "../components/OrderTable";
import type { Order, OrderInput, OrderUpdateInput } from "../types/order";

export function OrdersPage() {
  const {
    orders, isLoading, error,
    page, totalPages, setPage,
    sort, setSort,
    search, setSearch,
    setFilters,
    createOrder, updateOrder, deleteOrder,
  } = useOrders();

  const [editingOrder, setEditingOrder] = useState<Order | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [filterStatus, setFilterStatus] = useState('');
  const [statuses, setStatuses] = useState<string[]>([]);

  useEffect(() => {
    ordersApi.getOptions().then((o) => setStatuses(o.statuses));
  }, []);

  function openCreate() { setIsModalOpen(true); setEditingOrder(null); }
  function openEdit(o: Order) { setIsModalOpen(true); setEditingOrder(o); }
  function closeModal() { setIsModalOpen(false); setEditingOrder(null); }

  function handleStatusChange(value: string) {
    setFilterStatus(value);
    setFilters(value ? { status: `$eq:${value}` } : {});
    setPage(1);
  }

  async function handleSubmit(input: OrderInput | OrderUpdateInput) {
    if (editingOrder) await updateOrder(editingOrder.id, input as OrderUpdateInput);
    else await createOrder(input as OrderInput);
    closeModal();
  }

  async function handleDelete(id: number) {
    if (!globalThis.confirm("Delete this order?")) return;
    if (editingOrder?.id === id) closeModal();
    await deleteOrder(id);
  }

  return (
    <main className="container">
      <header><h1>Orders</h1></header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="filters">
        <input
          placeholder="Search user or product..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
        />
        <select value={filterStatus} onChange={(e) => handleStatusChange(e.target.value)}>
          <option value="">All statuses</option>
          {statuses.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="toolbar">
        <button className="btn btn-primary" onClick={openCreate}>+ Create Order</button>
      </div>

      {isLoading ? (
        <p className="loading">Loading...</p>
      ) : (
        <OrderTable orders={orders} sort={sort} onSort={setSort} onEdit={openEdit} onDelete={handleDelete} />
      )}

      <div className="pagination">
        <button className="btn" disabled={page === 1} onClick={() => setPage(page - 1)}>Previous</button>
        <span>Page {page} of {totalPages}</span>
        <button className="btn" disabled={page === totalPages} onClick={() => setPage(page + 1)}>Next</button>
      </div>

      {isModalOpen &&
        createPortal(
          <>
            <button className="modal-backdrop" onClick={closeModal} aria-label="Close modal" />
            <dialog className="modal" open>
              <OrderForm order={editingOrder} onSubmit={handleSubmit} onCancel={closeModal} />
            </dialog>
          </>,
          document.body,
        )}
    </main>
  );
}
