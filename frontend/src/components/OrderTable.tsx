// Presentation layer — list view
import { useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import type { Order } from "../types/order";
import type { SortState } from "../types/query";
import { toggleSort } from "../types/query";
import { sortIcons } from "../constants/ui";

interface OrderTableProps {
  orders: Order[];
  sort: SortState | null;
  onSort: (sort: SortState | null) => void;
  onEdit: (order: Order) => void;
  onDelete: (id: number) => void;
  onUploadInvoice: (id: number, file: File) => Promise<void>;
  onDeleteInvoice: (id: number) => Promise<void>;
  onDownloadInvoice: (id: number) => Promise<void>;
}

const currency = new Intl.NumberFormat("ro-RO", { style: "currency", currency: "RON" });

function SortIcon({ field, sort }: Readonly<{ field: string; sort: SortState | null }>) {
  if (sort?.field !== field) return <span className="sort-icon">{sortIcons.both}</span>;
  return <span className="sort-icon active">{sort.dir === 'ASC' ? sortIcons.asc : sortIcons.desc}</span>;
}

export function OrderTable({ orders, sort, onSort, onEdit, onDelete, onUploadInvoice, onDeleteInvoice, onDownloadInvoice }: Readonly<OrderTableProps>) {
  const { t } = useTranslation();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [pendingUploadId, setPendingUploadId] = useState<number | null>(null);

  function handleSort(field: string) {
    onSort(toggleSort(sort, field));
  }

  function triggerUpload(id: number) {
    setPendingUploadId(id);
    fileInputRef.current?.click();
  }

  async function handleFileChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (!file || pendingUploadId === null) return;
    await onUploadInvoice(pendingUploadId, file);
    setPendingUploadId(null);
    event.target.value = "";
  }

  if (orders.length === 0) {
    return <p className="empty">{t('orders.empty')}</p>;
  }

  return (
    <>
      <input
        ref={fileInputRef}
        type="file"
        accept=".pdf"
        style={{ display: "none" }}
        onChange={handleFileChange}
      />
      <table className="table">
        <thead>
          <tr>
            <th>#</th>
            <th>{t('table.user')}</th>
            <th>{t('table.product')}</th>
            <th className="num sortable" onClick={() => handleSort('quantity')}>
              {t('table.qty')} <SortIcon field="quantity" sort={sort} />
            </th>
            <th className="num sortable" onClick={() => handleSort('totalPrice')}>
              {t('table.total')} <SortIcon field="totalPrice" sort={sort} />
            </th>
            <th className="sortable" onClick={() => handleSort('status')}>
              {t('table.status')} <SortIcon field="status" sort={sort} />
            </th>
            <th className="sortable" onClick={() => handleSort('createdAt')}>
              {t('table.date')} <SortIcon field="createdAt" sort={sort} />
            </th>
            <th>{t('table.actions')}</th>
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
              <td>{order.status}</td>
              <td>{new Date(order.createdAt).toLocaleDateString()}</td>
              <td>
                <div className="row-actions">
                  <button className="btn btn-small" onClick={() => onEdit(order)}>{t('common.edit')}</button>
                  <button className="btn btn-small btn-danger" onClick={() => onDelete(order.id)}>{t('common.delete')}</button>
                  {order.invoiceUrl ? (
                    <>
                      <button className="btn btn-small" onClick={() => onDownloadInvoice(order.id)}>{t('orders.invoice.download')}</button>
                      <button className="btn btn-small btn-danger" onClick={() => onDeleteInvoice(order.id)}>{t('orders.invoice.remove')}</button>
                    </>
                  ) : (
                    <button className="btn btn-small" onClick={() => triggerUpload(order.id)}>{t('orders.invoice.upload')}</button>
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
