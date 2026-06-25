// Application layer
import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import toast from "react-hot-toast";
import { useProducts } from "../hooks/useProducts";
import { productsApi } from "../api/products";
import { ProductForm } from "../components/ProductForm";
import { ProductTable } from "../components/ProductTable";
import type { Product, ProductInput } from "../types/product";
import { ApiError } from "../api/errors";

const ENTITY = "Product";

export function ProductsPage() {
  const {
    products, isLoading, error,
    page, totalPages, setPage,
    sort, setSort,
    search, setSearch,
    setFilters,
    createProduct, updateProduct, deleteProduct,
  } = useProducts();

  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [filterCategory, setFilterCategory] = useState('');
  const [categories, setCategories] = useState<string[]>([]);

  useEffect(() => {
    productsApi.getOptions().then((o) => setCategories(o.categories));
  }, []);

  function openCreate() { setIsModalOpen(true); setEditingProduct(null); }
  function openEdit(p: Product) { setIsModalOpen(true); setEditingProduct(p); }
  function closeModal() { setIsModalOpen(false); setEditingProduct(null); }

  function handleCategoryChange(value: string) {
    setFilterCategory(value);
    setFilters(value ? { category: `$eq:${value}` } : {});
    setPage(1);
  }

  async function handleSubmit(input: ProductInput) {
    try {
      if (editingProduct) {
        await updateProduct(editingProduct.id, input, editingProduct.version);
        toast.success(`${ENTITY} updated`);
      } else {
        await createProduct(input);
        toast.success(`${ENTITY} created`);
      }
      closeModal();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleDelete(id: number) {
    if (!globalThis.confirm("Delete this product?")) return;
    try {
      if (editingProduct?.id === id) closeModal();
      await deleteProduct(id);
      toast.success(`${ENTITY} deleted`);
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  return (
    <main className="container">
      <header><h1>Products</h1></header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="filters">
        <input
          placeholder="Search name or category..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
        />
        <select value={filterCategory} onChange={(e) => handleCategoryChange(e.target.value)}>
          <option value="">All categories</option>
          {categories.map((c) => <option key={c} value={c}>{c}</option>)}
        </select>
      </div>

      <div className="toolbar">
        <button className="btn btn-primary" onClick={openCreate}>+ Add Product</button>
      </div>

      {isLoading ? (
        <p className="loading">Loading...</p>
      ) : (
        <ProductTable products={products} sort={sort} onSort={setSort} onEdit={openEdit} onDelete={handleDelete} />
      )}

      <div className="pagination">
        <button className="btn" disabled={page === 1} onClick={() => setPage(page - 1)}>Previous</button>
        <span>Page {page} of {totalPages}</span>
        <button className="btn" disabled={page === totalPages} onClick={() => setPage(page + 1)}>Next</button>
      </div>

      {isModalOpen &&
        createPortal(
          <>
            <button className="modal-backdrop" onClick={closeModal} aria-label="Close modal" />
            <dialog className="modal" open>
              <ProductForm product={editingProduct} onSubmit={handleSubmit} onCancel={closeModal} />
            </dialog>
          </>,
          document.body,
        )}
    </main>
  );
}
