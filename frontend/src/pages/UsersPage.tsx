// Application layer
import { useState } from "react";
import { createPortal } from "react-dom";
import toast from "react-hot-toast";
import { useTranslation } from "react-i18next";
import { Button } from "@mui/material";
import { useUsers } from "../hooks/useUsers";
import { ALLOWED_PAGE_SIZES } from "../constants/pagination";
import { useAuth } from "../context/AuthContext";
import { UserForm } from "../components/UserForm";
import { UserTable } from "../components/UserTable";
import type { User, UserInput } from "../types/user";
import { ApiError } from "../api/errors";

export function UsersPage() {
  const { t } = useTranslation();
  const { user: currentUser } = useAuth();
  const {
    users, isLoading, error,
    page, totalPages, setPage,
    pageSize, setPageSize,
    sort, setSort,
    search, setSearch,
    createUser, updateUser, deleteUser,
  } = useUsers();

  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  function openCreate() { setIsModalOpen(true); setEditingUser(null); }
  function openEdit(u: User) { setIsModalOpen(true); setEditingUser(u); }
  function closeModal() { setIsModalOpen(false); setEditingUser(null); }

  async function handleSubmit(input: UserInput) {
    try {
      if (editingUser) {
        await updateUser(editingUser.id, input, editingUser.version);
        toast.success(t('users.updated'));
      } else {
        await createUser(input);
        toast.success(t('users.created'));
      }
      closeModal();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleDelete(id: number) {
    if (!globalThis.confirm(t('users.deleteConfirm'))) return;
    try {
      if (editingUser?.id === id) closeModal();
      await deleteUser(id);
      toast.success(t('users.deleted'));
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  return (
    <main className="container">
      <header><h1>{t('users.title')}</h1></header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="search-bar">
        <input
          placeholder={t('users.search')}
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
        />
      </div>

      <div className="toolbar">
        <Button variant="contained" onClick={openCreate}>{t('users.add')}</Button>
      </div>

      {isLoading ? (
        <p className="loading">{t('common.loading')}</p>
      ) : (
        <UserTable users={users} sort={sort} onSort={setSort} onEdit={openEdit} onDelete={handleDelete} />
      )}

      <div className="pagination">
        <button className="btn" disabled={page === 1} onClick={() => setPage(page - 1)}>{t('pagination.previous')}</button>
        <span>{t('pagination.page', { page, total: totalPages })}</span>
        <button className="btn" disabled={page === totalPages} onClick={() => setPage(page + 1)}>{t('pagination.next')}</button>
        <select value={pageSize} onChange={(e) => { setPageSize(Number(e.target.value)); setPage(1); }}>
          {ALLOWED_PAGE_SIZES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      {isModalOpen &&
        createPortal(
          <>
            <button className="modal-backdrop" onClick={closeModal} aria-label="Close modal" />
            <dialog className="modal" open>
              <UserForm user={editingUser} currentUser={currentUser!} onSubmit={handleSubmit} onCancel={closeModal} />
            </dialog>
          </>,
          document.body,
        )}
    </main>
  );
}
