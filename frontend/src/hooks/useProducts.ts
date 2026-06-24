// Service layer — products
import { useCallback, useEffect, useState } from 'react';
import { productsApi } from '../api/products';
import { ApiError } from '../api/errors';
import type { Product, ProductInput } from '../types/product';
import type { SortState } from '../types/query';
import { toSortBy } from '../types/query';
import { PAGE_SIZE } from '../constants/pagination';

export function useProducts() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // --- Sorting ---
  const [sort, setSort] = useState<SortState | null>(null);

  // --- Filtering ---
  const [search, setSearch] = useState('');
  const [filters, setFilters] = useState<Record<string, string>>({});

  const loadProducts = useCallback(async function loadProducts(
    p: number,
    s: SortState | null,
    search: string,
    filters: Record<string, string>,
  ) {
    setIsLoading(true);
    setError(null);
    try {
      const data = await productsApi.getAll(p, PAGE_SIZE, toSortBy(s), search || undefined, Object.keys(filters).length ? filters : undefined);
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
    if (page === 1) void loadProducts(1, sort, search, filters);
    else setPage(1);
  }, [loadProducts, page, sort, search, filters]);

  const updateProduct = useCallback(async function updateProduct(id: number, input: ProductInput, version: number) {
    const updated = await productsApi.update(id, input, version);
    setProducts((prev) => prev.map((p) => (p.id === id ? updated : p)));
  }, []);

  const deleteProduct = useCallback(async function deleteProduct(id: number) {
    await productsApi.remove(id);
    const newPage = products.length === 1 && page > 1 ? page - 1 : page;
    if (newPage === page) void loadProducts(page, sort, search, filters);
    else setPage(newPage);
  }, [loadProducts, page, products.length, sort, search, filters]);

  useEffect(() => {
    void loadProducts(page, sort, search, filters);
  }, [loadProducts, page, sort, search, filters]);

  return {
    products, isLoading, error,
    page, totalPages: Math.max(1, Math.ceil(totalCount / PAGE_SIZE)), setPage,
    sort, setSort,
    search, setSearch,
    filters, setFilters,
    createProduct, updateProduct, deleteProduct,
  };
}
