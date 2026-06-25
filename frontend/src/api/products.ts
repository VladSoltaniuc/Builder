// API layer — products
import { httpCore } from './httpCore';
import { httpFile } from './httpFile';
import { buildPagedParams } from './buildPagedParams';
import type { Product, ProductInput } from '../types/product';
import type { PagedResponse } from '../types/pagination';
import type { ProductOptions } from '../types/options';

const RESOURCE = '/products';

export const productsApi = {
  getOptions: () => httpCore.get<ProductOptions>(`${RESOURCE}/options`),

  getAll: (page: number, pageSize: number, sortBy?: string, search?: string, filters?: Record<string, string>) =>
    httpCore.get<PagedResponse<Product>>(`${RESOURCE}?${buildPagedParams(page, pageSize, sortBy, search, filters)}`),

  create: (input: ProductInput) => httpCore.post<Product>(RESOURCE, input),

  update: (id: number, input: ProductInput, version: number) =>
    httpCore.put<Product>(`${RESOURCE}/${id}`, { ...input, version }),

  remove: (id: number) => httpCore.delete(`${RESOURCE}/${id}`),

  uploadImage: (id: number, file: File) => httpFile.upload<Product>(`${RESOURCE}/${id}/image`, file),

  deleteImage: (id: number) => httpFile.delete(`${RESOURCE}/${id}/image`),
};
