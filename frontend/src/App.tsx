// UI orchestration layer
import { useState } from "react";
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

  async function handleSubmit(input: ProductInput) {
    if (editing) {
      await updateProduct(editing.id, input, editing.version);
      setEditing(null);
    } else {
      await createProduct(input);
    }
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Sigur ștergi acest produs?")) {
      return;
    }
    if (editing?.id === id) {
      setEditing(null);
    }
    await deleteProduct(id);
  }

  return (
    <main className="container">
      <header>
        <h1>Catalog de produse</h1>
        <p className="subtitle">Demo CRUD: React + .NET Minimal API</p>
      </header>

      <ProductForm
        editing={editing}
        onSubmit={handleSubmit}
        onCancel={() => setEditing(null)}
      />

      {error && <p className="error">⚠️ {error}</p>}

      {isLoading ? (
        <p className="loading">Se încarcă produsele...</p>
      ) : (
        <ProductTable
          products={products}
          onEdit={setEditing}
          onDelete={handleDelete}
        />
      )}

      <div className="pagination">
        <button
          className="btn"
          disabled={page === 1}
          onClick={() => setPage(page - 1)}
        >
          Anterior
        </button>
        <span>
          Pagina {page} din {totalPages}
        </span>
        <button
          className="btn"
          disabled={page === totalPages}
          onClick={() => setPage(page + 1)}
        >
          Următor
        </button>
      </div>
    </main>
  );
}
