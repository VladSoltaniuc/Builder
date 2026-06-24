// Service layer — products
import { useCallback, useEffect, useState } from 'react';
import { productsApi } from '../api/products';
import { ApiError } from '../api/errors';
import type { Product, ProductInput } from '../types/product';
import { PAGE_SIZE } from '../constants/pagination';

export function useProducts() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const loadProducts = useCallback(async function loadProducts(p: number) {
    setIsLoading(true);
    setError(null);
    try {
      const data = await productsApi.getAll(p, PAGE_SIZE);
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
    // Go to page 1 after create — triggers useEffect which reloads
    if (page === 1) void loadProducts(1);
    else setPage(1);
  }, [loadProducts, page]);

  const updateProduct = useCallback(async function updateProduct(id: number, input: ProductInput, version: number) {
    const updated = await productsApi.update(id, input, version);
    setProducts((prev) => prev.map((p) => (p.id === id ? updated : p)));
  }, []);

  const deleteProduct = useCallback(async function deleteProduct(id: number) {
    await productsApi.remove(id);
    // If this was the last item on a non-first page, go back one page
    const newPage = products.length === 1 && page > 1 ? page - 1 : page;
    if (newPage === page) void loadProducts(page);
    else setPage(newPage);
  }, [loadProducts, page, products.length]);

  // Reload whenever page changes
  useEffect(() => {
    void loadProducts(page);
  }, [loadProducts, page]);

  return {
    products,
    isLoading,
    error,
    page,
    totalPages: Math.max(1, Math.ceil(totalCount / PAGE_SIZE)),
    setPage,
    createProduct,
    updateProduct,
    deleteProduct,
  };
}
