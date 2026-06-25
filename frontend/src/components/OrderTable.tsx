// Presentation layer — list view
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
}

const currency = new Intl.NumberFormat("ro-RO", { style: "currency", currency: "RON" });

function SortIcon({ field, sort }: Readonly<{ field: string; sort: SortState | null }>) {
  if (sort?.field !== field) return <span className="sort-icon">{sortIcons.both}</span>;
  return <span className="sort-icon active">{sort.dir === 'ASC' ? sortIcons.asc : sortIcons.desc}</span>;
}

export function OrderTable({ orders, sort, onSort, onEdit, onDelete }: Readonly<OrderTableProps>) {
  const { t } = useTranslation();

  function handleSort(field: string) {
    onSort(toggleSort(sort, field));
  }

  if (orders.length === 0) {
    return <p className="empty">{t('orders.empty')}</p>;
  }

  return (
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
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
