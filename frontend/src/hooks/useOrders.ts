// Service layer — orders
import { useCallback, useEffect, useState } from 'react';
import { ordersApi } from '../api/orders';
import { ApiError } from '../api/errors';
import type { Order, OrderInput, OrderUpdateInput } from '../types/order';
import { PAGE_SIZE } from '../constants/pagination';

export function useOrders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const loadOrders = useCallback(async function loadOrders(p: number) {
    setIsLoading(true);
    setError(null);
    try {
      const data = await ordersApi.getAll(p, PAGE_SIZE);
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
    if (page === 1) void loadOrders(1);
    else setPage(1);
  }, [loadOrders, page]);

  const updateOrder = useCallback(async function updateOrder(id: number, input: OrderUpdateInput) {
    const updated = await ordersApi.update(id, input);
    setOrders((prev) => prev.map((o) => (o.id === id ? updated : o)));
  }, []);

  const deleteOrder = useCallback(async function deleteOrder(id: number) {
    await ordersApi.remove(id);
    const newPage = orders.length === 1 && page > 1 ? page - 1 : page;
    if (newPage === page) void loadOrders(page);
    else setPage(newPage);
  }, [loadOrders, page, orders.length]);

  useEffect(() => {
    void loadOrders(page);
  }, [loadOrders, page]);

  return {
    orders,
    isLoading,
    error,
    page,
    totalPages: Math.max(1, Math.ceil(totalCount / PAGE_SIZE)),
    setPage,
    createOrder,
    updateOrder,
    deleteOrder,
  };
}
