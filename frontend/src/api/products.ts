// API layer — products
import { httpCore } from './httpCore';
import type { Product, ProductInput } from '../types/product';
import type { PagedResponse } from '../types/pagination';

const RESOURCE = '/products';

export const productsApi = {
  getAll: (page: number, pageSize: number) =>
    httpCore.get<PagedResponse<Product>>(`${RESOURCE}?page=${page}&pageSize=${pageSize}`),

  create: (input: ProductInput) => httpCore.post<Product>(RESOURCE, input),

  update: (id: number, input: ProductInput, version: number) =>
    httpCore.put<Product>(`${RESOURCE}/${id}`, { ...input, version }),

  remove: (id: number) => httpCore.delete(`${RESOURCE}/${id}`),
};
