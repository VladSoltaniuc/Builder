// Application layer
import { useState } from "react";
import { createPortal } from "react-dom";
import toast from "react-hot-toast";
import { useTranslation } from "react-i18next";
import { Button } from "@mui/material";
import { useProducts } from "../hooks/useProducts";
import { ALLOWED_PAGE_SIZES } from "../constants/pagination";
import { ProductForm } from "../components/ProductForm";
import { ProductTable } from "../components/ProductTable";
import { ExcelExportDialog } from "../components/ExcelExportDialog";
import { ExcelImportDialog } from "../components/ExcelImportDialog";
import type { Product, ProductInput } from "../types/product";
import { ApiError } from "../api/errors";

export function ProductsPage() {
  const { t } = useTranslation();
  const {
    products, isLoading, error,
    page, totalPages, setPage,
    pageSize, setPageSize,
    sort, setSort,
    search, setSearch,
    createProduct, updateProduct, deleteProduct,
    uploadImage, deleteImage, refresh,
  } = useProducts();

  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isExportOpen, setIsExportOpen] = useState(false);
  const [isImportOpen, setIsImportOpen] = useState(false);

  function openCreate() { setIsModalOpen(true); setEditingProduct(null); }
  function openEdit(p: Product) { setIsModalOpen(true); setEditingProduct(p); }
  function closeModal() { setIsModalOpen(false); setEditingProduct(null); }

  async function handleSubmit(input: ProductInput) {
    try {
      if (editingProduct) {
        await updateProduct(editingProduct.id, input, editingProduct.version);
        toast.success(t('products.updated'));
      } else {
        await createProduct(input);
        toast.success(t('products.created'));
      }
      closeModal();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleDelete(id: number) {
    if (!globalThis.confirm(t('products.deleteConfirm'))) return;
    try {
      if (editingProduct?.id === id) closeModal();
      await deleteProduct(id);
      toast.success(t('products.deleted'));
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleUploadImage(file: File) {
    try {
      const updated = await uploadImage(editingProduct!.id, file);
      setEditingProduct(updated);
      return updated;
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
      throw err;
    }
  }

  async function handleDeleteImage() {
    try {
      await deleteImage(editingProduct!.id);
      setEditingProduct((prev) => prev ? { ...prev, imageUrl: undefined } : null);
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
      throw err;
    }
  }

  return (
    <main className="container">
      <header><h1>{t('products.title')}</h1></header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="search-bar">
        <input
          placeholder={t('products.search')}
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
        />
      </div>

      <div className="toolbar">
        <Button variant="outlined" size="small" onClick={() => setIsImportOpen(true)}>{t('excel.import')}</Button>
        <Button variant="outlined" size="small" onClick={() => setIsExportOpen(true)}>{t('excel.export')}</Button>
        <Button variant="contained" onClick={openCreate}>{t('products.add')}</Button>
      </div>

      {isLoading ? (
        <p className="loading">{t('common.loading')}</p>
      ) : (
        <ProductTable products={products} sort={sort} onSort={setSort} onEdit={openEdit} onDelete={handleDelete} />
      )}

      <div className="pagination">
        <button className="btn" disabled={page === 1} onClick={() => setPage(page - 1)}>{t('pagination.previous')}</button>
        <span>{t('pagination.page', { page, total: totalPages })}</span>
        <button className="btn" disabled={page === totalPages} onClick={() => setPage(page + 1)}>{t('pagination.next')}</button>
        <select value={pageSize} onChange={(e) => { setPageSize(Number(e.target.value)); setPage(1); }}>
          {ALLOWED_PAGE_SIZES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      {isExportOpen && <ExcelExportDialog onClose={() => setIsExportOpen(false)} />}
      {isImportOpen && <ExcelImportDialog onClose={() => setIsImportOpen(false)} onImported={refresh} />}

      {isModalOpen &&
        createPortal(
          <>
            <button className="modal-backdrop" onClick={closeModal} aria-label="Close modal" />
            <dialog className="modal" open>
              <ProductForm
                product={editingProduct}
                onSubmit={handleSubmit}
                onCancel={closeModal}
                onUploadImage={editingProduct ? handleUploadImage : undefined}
                onDeleteImage={editingProduct ? handleDeleteImage : undefined}
              />
            </dialog>
          </>,
          document.body,
        )}
    </main>
  );
}
