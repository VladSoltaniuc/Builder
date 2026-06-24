// API layer — orders
import { httpCore } from './httpCore';
import { buildPagedParams } from './buildPagedParams';
import type { Order, OrderInput, OrderUpdateInput } from '../types/order';
import type { PagedResponse } from '../types/pagination';
import type { OrderOptions } from '../types/options';

const RESOURCE = '/orders';

export const ordersApi = {
  getOptions: () => httpCore.get<OrderOptions>(`${RESOURCE}/options`),

  getAll: (page: number, pageSize: number, sortBy?: string, search?: string, filters?: Record<string, string>) =>
    httpCore.get<PagedResponse<Order>>(`${RESOURCE}?${buildPagedParams(page, pageSize, sortBy, search, filters)}`),

  getById: (id: number) => httpCore.get<Order>(`${RESOURCE}/${id}`),

  create: (input: OrderInput) => httpCore.post<Order>(RESOURCE, input),

  update: (id: number, input: OrderUpdateInput) => httpCore.put<Order>(`${RESOURCE}/${id}`, input),

  remove: (id: number) => httpCore.delete(`${RESOURCE}/${id}`),
};
