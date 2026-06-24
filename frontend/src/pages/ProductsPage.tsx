// Application layer
import { useState } from "react";
import { createPortal } from "react-dom";
import { useProducts } from "../hooks/useProducts";
import { ProductForm } from "../components/ProductForm";
import { ProductTable } from "../components/ProductTable";
import type { Product, ProductInput } from "../types/product";

export function ProductsPage() {
  const {
    products,
    isLoading,
    error,
    page,
    totalPages,
    setPage,
    createProduct,
    updateProduct,
    deleteProduct,
  } = useProducts();

  const [product, setEditing] = useState<Product | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  function openCreate() {
    setIsModalOpen(true);
    setEditing(null);
  }

  function openEdit(p: Product) {
    setIsModalOpen(true);
    setEditing(p);
  }

  function closeModal() {
    setIsModalOpen(false);
    setEditing(null);
  }

  async function handleSubmit(input: ProductInput) {
    if (product) {
      await updateProduct(product.id, input, product.version);
    } else {
      await createProduct(input);
    }
    closeModal();
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Delete this product?")) return;
    if (product?.id === id) closeModal();
    await deleteProduct(id);
  }

  return (
    <main className="container">
      <header>
        <h1>Products</h1>
      </header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="toolbar">
        <button className="btn btn-primary" onClick={openCreate}>
          + Add Product
        </button>
      </div>

      {isLoading ? (
        <p className="loading">Loading...</p>
      ) : (
        <ProductTable products={products} onEdit={openEdit} onDelete={handleDelete} />
      )}

      <div className="pagination">
        <button className="btn" disabled={page === 1} onClick={() => setPage(page - 1)}>
          Previous
        </button>
        <span>Page {page} of {totalPages}</span>
        <button className="btn" disabled={page === totalPages} onClick={() => setPage(page + 1)}>
          Next
        </button>
      </div>

      {isModalOpen &&
        createPortal(
          <div className="modal-backdrop" onClick={closeModal}>
            <div
              className="modal"
              role="dialog"
              aria-modal="true"
              onClick={(e) => e.stopPropagation()}
              onKeyDown={(e) => e.stopPropagation()}
            >
              <ProductForm product={product} onSubmit={handleSubmit} onCancel={closeModal} />
            </div>
          </div>,
          document.body,
        )}
    </main>
  );
}
