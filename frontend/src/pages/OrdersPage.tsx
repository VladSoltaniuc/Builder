// Application layer
import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import toast from "react-hot-toast";
import { useTranslation } from "react-i18next";
import { Button } from "@mui/material";
import { useOrders } from "../hooks/useOrders";
import { ordersApi } from "../api/orders";
import { OrderForm } from "../components/OrderForm";
import { OrderTable } from "../components/OrderTable";
import type { Order, OrderInput, OrderUpdateInput } from "../types/order";
import { ApiError } from "../api/errors";

export function OrdersPage() {
  const { t } = useTranslation();
  const {
    orders, isLoading, error,
    page, totalPages, setPage,
    sort, setSort,
    search, setSearch,
    setFilters,
    createOrder, updateOrder, deleteOrder,
    uploadInvoice, deleteInvoice, downloadInvoice,
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
    try {
      if (editingOrder) {
        await updateOrder(editingOrder.id, input as OrderUpdateInput);
        toast.success(t('orders.updated'));
      } else {
        await createOrder(input as OrderInput);
        toast.success(t('orders.created'));
      }
      closeModal();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleDelete(id: number) {
    if (!globalThis.confirm(t('orders.deleteConfirm'))) return;
    try {
      if (editingOrder?.id === id) closeModal();
      await deleteOrder(id);
      toast.success(t('orders.deleted'));
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleUploadInvoice(id: number, file: File) {
    try {
      await uploadInvoice(id, file);
      toast.success(t('orders.invoice.uploaded'));
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleDeleteInvoice(id: number) {
    try {
      await deleteInvoice(id);
      toast.success(t('orders.invoice.removed'));
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleDownloadInvoice(id: number) {
    try {
      await downloadInvoice(id);
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  return (
    <main className="container">
      <header><h1>{t('orders.title')}</h1></header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="filters">
        <input
          placeholder={t('orders.search')}
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
        />
        <select value={filterStatus} onChange={(e) => handleStatusChange(e.target.value)}>
          <option value="">{t('orders.allStatuses')}</option>
          {statuses.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="toolbar">
        <Button variant="contained" onClick={openCreate}>{t('orders.add')}</Button>
      </div>

      {isLoading ? (
        <p className="loading">{t('common.loading')}</p>
      ) : (
        <OrderTable
          orders={orders}
          sort={sort}
          onSort={setSort}
          onEdit={openEdit}
          onDelete={handleDelete}
          onUploadInvoice={handleUploadInvoice}
          onDeleteInvoice={handleDeleteInvoice}
          onDownloadInvoice={handleDownloadInvoice}
        />
      )}

      <div className="pagination">
        <button className="btn" disabled={page === 1} onClick={() => setPage(page - 1)}>{t('pagination.previous')}</button>
        <span>{t('pagination.page', { page, total: totalPages })}</span>
        <button className="btn" disabled={page === totalPages} onClick={() => setPage(page + 1)}>{t('pagination.next')}</button>
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
