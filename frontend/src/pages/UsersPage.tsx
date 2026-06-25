// Application layer
import { useState } from "react";
import { createPortal } from "react-dom";
import toast from "react-hot-toast";
import { useUsers } from "../hooks/useUsers";
import { UserForm } from "../components/UserForm";
import { UserTable } from "../components/UserTable";
import type { User, UserInput } from "../types/user";
import { ApiError } from "../api/errors";
import { Button } from "@mui/material";

const ENTITY = "User";

export function UsersPage() {
  const {
    users, isLoading, error,
    page, totalPages, setPage,
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
        toast.success(`${ENTITY} updated`);
      } else {
        await createUser(input);
        toast.success(`${ENTITY} created`);
      }
      closeModal();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  async function handleDelete(id: number) {
    if (!globalThis.confirm("Delete this user?")) return;
    try {
      if (editingUser?.id === id) closeModal();
      await deleteUser(id);
      toast.success(`${ENTITY} deleted`);
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : "Something went wrong");
    }
  }

  return (
    <main className="container">
      <header><h1>Users</h1></header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="filters">
        <input
          placeholder="Search name or email..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
        />
      </div>

      <div className="toolbar">
        <Button variant="contained" onClick={openCreate}>+ Add User</Button>
      </div>

      {isLoading ? (
        <p className="loading">Loading...</p>
      ) : (
        <UserTable users={users} sort={sort} onSort={setSort} onEdit={openEdit} onDelete={handleDelete} />
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
              <UserForm user={editingUser} onSubmit={handleSubmit} onCancel={closeModal} />
            </dialog>
          </>,
          document.body,
        )}
    </main>
  );
}
