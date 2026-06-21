// UI orchestration layer
import { useState } from "react";
import { createPortal } from "react-dom";
import { useProducts } from "./hooks/useProducts";
import { ProductForm } from "./components/ProductForm";
import { ProductTable } from "./components/ProductTable";
import type { Product, ProductInput } from "./types/product";

export function App() {
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

  const [editing, setEditing] = useState<Product | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  function openCreate() {
    setEditing(null);
    setIsModalOpen(true);
  }

  function openEdit(product: Product) {
    setEditing(product);
    setIsModalOpen(true);
  }

  function closeModal() {
    setIsModalOpen(false);
    setEditing(null);
  }

  async function handleSubmit(input: ProductInput) {
    if (editing) {
      await updateProduct(editing.id, input, editing.version);
    } else {
      await createProduct(input);
    }
    closeModal();
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Sigur ștergi acest produs?")) {
      return;
    }
    if (editing?.id === id) {
      closeModal();
    }
    await deleteProduct(id);
  }

  return (
    <main className="container">
      <header>
        <h1>Catalog de produse</h1>
        <p className="subtitle">Demo CRUD: React + .NET Minimal API</p>
      </header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="toolbar">
        <button className="btn btn-primary" onClick={openCreate}>
          + Adaugă produs
        </button>
      </div>

      {isLoading ? (
        <p className="loading">Se încarcă produsele...</p>
      ) : (
        <ProductTable products={products} onEdit={openEdit} onDelete={handleDelete} />
      )}

      <div className="pagination">
        <button className="btn" disabled={page === 1} onClick={() => setPage(page - 1)}>
          Anterior
        </button>
        <span>Pagina {page} din {totalPages}</span>
        <button className="btn" disabled={page === totalPages} onClick={() => setPage(page + 1)}>
          Următor
        </button>
      </div>

      {isModalOpen && createPortal(
        <div className="modal-backdrop" onClick={closeModal}>
          <div
            className="modal"
            role="dialog"
            aria-modal="true"
            onClick={(e) => e.stopPropagation()}
            onKeyDown={(e) => e.stopPropagation()}
          >
            <ProductForm editing={editing} onSubmit={handleSubmit} onCancel={closeModal} />
          </div>
        </div>,
        document.body
      )}
    </main>
  );
}
