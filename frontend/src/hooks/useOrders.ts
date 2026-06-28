// Service layer — orders
import { useCallback, useEffect, useState } from 'react';
import { ordersApi } from '../api/orders';
import { ApiError } from '../api/errors';
import type { Order, OrderInput, OrderUpdateInput } from '../types/order';
import type { SortState } from '../types/query';
import { toSortBy } from '../types/query';
import { PAGE_SIZE } from '../constants/pagination';

export function useOrders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // --- Sorting ---
  const [sort, setSort] = useState<SortState | null>(null);

  // --- Filtering ---
  const [search, setSearch] = useState('');
  const [filters, setFilters] = useState<Record<string, string>>({});

  const loadOrders = useCallback(async function loadOrders(
    p: number,
    s: SortState | null,
    search: string,
    filters: Record<string, string>,
  ) {
    setIsLoading(true);
    setError(null);
    try {
      const data = await ordersApi.getAll(p, PAGE_SIZE, toSortBy(s), search || undefined, Object.keys(filters).length ? filters : undefined);
      setOrders(data.items);
      setTotalCount(data.totalCount);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : ApiError.fromStatus(1).message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const createOrder = useCallback(async function createOrder(input: OrderInput) {
    await ordersApi.create(input);
    if (page === 1) void loadOrders(1, sort, search, filters);
    else setPage(1);
  }, [loadOrders, page, sort, search, filters]);

  const updateOrder = useCallback(async function updateOrder(id: number, input: OrderUpdateInput) {
    const updated = await ordersApi.update(id, input);
    setOrders((prev) => prev.map((o) => (o.id === id ? updated : o)));
  }, []);

  const patchOrder = useCallback((updated: Order) => {
    setOrders((prev) => prev.map((o) => (o.id === updated.id ? updated : o)));
  }, []);

  const deleteOrder = useCallback(async function deleteOrder(id: number) {
    await ordersApi.remove(id);
    const newPage = orders.length === 1 && page > 1 ? page - 1 : page;
    if (newPage === page) void loadOrders(page, sort, search, filters);
    else setPage(newPage);
  }, [loadOrders, page, orders.length, sort, search, filters]);

  const uploadInvoice = useCallback(async function uploadInvoice(id: number, file: File) {
    const updated = await ordersApi.uploadInvoice(id, file);
    setOrders((prev) => prev.map((o) => (o.id === id ? updated : o)));
  }, []);

  const deleteInvoice = useCallback(async function deleteInvoice(id: number) {
    await ordersApi.deleteInvoice(id);
    setOrders((prev) => prev.map((o) => (o.id === id ? { ...o, invoiceUrl: undefined } : o)));
  }, []);

  const downloadInvoice = useCallback(async function downloadInvoice(id: number) {
    const blob = await ordersApi.downloadInvoice(id);
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `invoice-${id}.pdf`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
  }, []);

  useEffect(() => {
    void loadOrders(page, sort, search, filters);
  }, [loadOrders, page, sort, search, filters]);

  return {
    orders, isLoading, error,
    page, totalPages: Math.max(1, Math.ceil(totalCount / PAGE_SIZE)), setPage,
    sort, setSort,
    search, setSearch,
    filters, setFilters,
    createOrder, updateOrder, patchOrder, deleteOrder,
    uploadInvoice, deleteInvoice, downloadInvoice,
  };
}
