// Application layer
import { useState } from "react";
import { createPortal } from "react-dom";
import { useUsers } from "../hooks/useUsers";
import { UserForm } from "../components/UserForm";
import { UserTable } from "../components/UserTable";
import type { User, UserInput } from "../types/user";

export function UsersPage() {
  const {
    users,
    isLoading,
    error,
    page,
    totalPages,
    setPage,
    createUser,
    updateUser,
    deleteUser,
  } = useUsers();

  const [user, setEditing] = useState<User | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  function openCreate() {
    setIsModalOpen(true);
    setEditing(null);
  }

  function openEdit(u: User) {
    setIsModalOpen(true);
    setEditing(u);
  }

  function closeModal() {
    setIsModalOpen(false);
    setEditing(null);
  }

  async function handleSubmit(input: UserInput) {
    if (user) {
      await updateUser(user.id, input, user.version);
    } else {
      await createUser(input);
    }
    closeModal();
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Delete this user?")) return;
    if (user?.id === id) closeModal();
    await deleteUser(id);
  }

  return (
    <main className="container">
      <header>
        <h1>Users</h1>
      </header>

      {error && <p className="error">⚠️ {error}</p>}

      <div className="toolbar">
        <button className="btn btn-primary" onClick={openCreate}>
          + Add User
        </button>
      </div>

      {isLoading ? (
        <p className="loading">Loading...</p>
      ) : (
        <UserTable users={users} onEdit={openEdit} onDelete={handleDelete} />
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
              <UserForm user={user} onSubmit={handleSubmit} onCancel={closeModal} />
            </div>
          </div>,
          document.body,
        )}
    </main>
  );
}
