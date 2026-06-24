// API layer — orders
import { httpCore } from './httpCore';
import type { Order, OrderInput, OrderUpdateInput } from '../types/order';
import type { PagedResponse } from '../types/pagination';

const RESOURCE = '/orders';

export const ordersApi = {
  getAll: (page: number, pageSize: number) =>
    httpCore.get<PagedResponse<Order>>(`${RESOURCE}?page=${page}&pageSize=${pageSize}`),

  getById: (id: number) =>
    httpCore.get<Order>(`${RESOURCE}/${id}`),

  create: (input: OrderInput) =>
    httpCore.post<Order>(RESOURCE, input),

  update: (id: number, input: OrderUpdateInput) =>
    httpCore.put<Order>(`${RESOURCE}/${id}`, input),

  remove: (id: number) =>
    httpCore.delete(`${RESOURCE}/${id}`),
};
