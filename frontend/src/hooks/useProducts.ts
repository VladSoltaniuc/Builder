// Service layer - products
import { useCallback, useEffect, useState } from 'react';
import { productsApi } from '../api/products';
import { ApiError } from '../api/errors';
import type { Product, ProductInput } from '../types/product';
import type { SortState } from '../types/query';
import { toSortBy } from '../types/query';
import { DEFAULT_PAGE_SIZE } from '../constants/pagination';

export function useProducts() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [totalCount, setTotalCount] = useState(0);

  // --- Sorting ---
  const [sort, setSort] = useState<SortState | null>(null);

  // --- Search ---
  const [search, setSearch] = useState('');

  const loadProducts = useCallback(async function loadProducts(
    p: number,
    ps: number,
    s: SortState | null,
    search: string,
  ) {
    setIsLoading(true);
    setError(null);
    try {
      const data = await productsApi.getAll(p, ps, toSortBy(s), search || undefined);
      setProducts(data.items);
      setTotalCount(data.totalCount);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : ApiError.fromStatus(1).message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const createProduct = useCallback(async function createProduct(input: ProductInput) {
    await productsApi.create(input);
    if (page === 1) void loadProducts(1, pageSize, sort, search);
    else setPage(1);
  }, [loadProducts, page, pageSize, sort, search]);

  const updateProduct = useCallback(async function updateProduct(id: number, input: ProductInput, version: number) {
    const updated = await productsApi.update(id, input, version);
    setProducts((prev) => prev.map((p) => (p.id === id ? updated : p)));
  }, []);

  const deleteProduct = useCallback(async function deleteProduct(id: number) {
    await productsApi.remove(id);
    const newPage = products.length === 1 && page > 1 ? page - 1 : page;
    if (newPage === page) void loadProducts(page, pageSize, sort, search);
    else setPage(newPage);
  }, [loadProducts, page, pageSize, products.length, sort, search]);

  const uploadImage = useCallback(async function uploadImage(id: number, file: File) {
    const updated = await productsApi.uploadImage(id, file);
    setProducts((prev) => prev.map((p) => (p.id === id ? updated : p)));
    return updated;
  }, []);

  const deleteImage = useCallback(async function deleteImage(id: number) {
    await productsApi.deleteImage(id);
    setProducts((prev) => prev.map((p) => (p.id === id ? { ...p, imageUrl: undefined } : p)));
  }, []);

  const refresh = useCallback(() => {
    void loadProducts(page, pageSize, sort, search);
  }, [loadProducts, page, pageSize, sort, search]);

  useEffect(() => {
    void loadProducts(page, pageSize, sort, search);
  }, [loadProducts, page, pageSize, sort, search]);

  return {
    products, isLoading, error,
    page, totalPages: Math.max(1, Math.ceil(totalCount / pageSize)), setPage,
    pageSize, setPageSize,
    sort, setSort,
    search, setSearch,
    createProduct, updateProduct, deleteProduct,
    uploadImage, deleteImage, refresh,
  };
}
