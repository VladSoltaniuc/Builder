// Products - API layer
import { httpCore } from './httpCore';
import type { Product, ProductInput } from '../types/product';
import type { PagedResponse } from '../types/pagination';

const RESOURCE = '/products';

export const productsApi = {
  /** GET /api/products?page=1&pageSize=10 */
  getAll: (page: number, pageSize: number) =>
    httpCore.get<PagedResponse<Product>>(`${RESOURCE}?page=${page}&pageSize=${pageSize}`),

  /** POST /api/products */
  create: (input: ProductInput) => httpCore.post<Product>(RESOURCE, input),

  /** PUT /api/products/{id} */
  update: (id: number, input: ProductInput, version: number) =>
    httpCore.put<Product>(`${RESOURCE}/${id}`, { ...input, version }),

  /** DELETE /api/products/{id} */
  remove: (id: number) => httpCore.delete(`${RESOURCE}/${id}`),
};
