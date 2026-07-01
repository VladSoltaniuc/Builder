// Application layer
import { useState } from "react";
import { createPortal } from "react-dom";
import toast from "react-hot-toast";
import { useTranslation } from "react-i18next";
import { Button } from "@mui/material";
import { useOrders } from "../hooks/useOrders";
import { ALLOWED_PAGE_SIZES } from "../constants/pagination";
import { useOrderHub } from "../hooks/useOrderHub";
import { OrderForm } from "../components/OrderForm";
import { OrderTable } from "../components/OrderTable";
import type { Order, OrderInput, OrderUpdateInput } from "../types/order";
import { ApiError } from "../api/errors";

export function OrdersPage() {
  const { t } = useTranslation();
  const {
    orders, isLoading, error,
    page, totalPages, setPage,
    pageSize, setPageSize,
    sort, setSort,
    search, setSearch,
    createOrder, updateOrder, patchOrder, deleteOrder,
    uploadInvoice, deleteInvoice, downloadInvoice,
  } = useOrders();

  // Live order-status updates pushed from the server over SignalR
  // If an order visible on this page is updated by another session, we patch
  // it in-place and show a brief toast so the user notices without refreshing
  useOrderHub((updated) => {
    const current = orders.find((o) => o.id === updated.id);
    if (current && current.version < updated.version) {
      toast(`Live: Order #${updated.id} → ${updated.status}`, { id: `hub-order-${updated.id}` });
    }
    patchOrder(updated);
  });

  const [editingOrder, setEditingOrder] = useState<Order | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  function openCreate() { setIsModalOpen(true); setEditingOrder(null); }
  function openEdit(o: Order) { setIsModalOpen(true); setEditingOrder(o); }
  function closeModal() { setIsModalOpen(false); setEditingOrder(null); }

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

      <div className="search-bar">
        <input
          placeholder={t('orders.search')}
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
        />
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
        <select value={pageSize} onChange={(e) => { setPageSize(Number(e.target.value)); setPage(1); }}>
          {ALLOWED_PAGE_SIZES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
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
