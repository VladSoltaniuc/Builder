// Presentation layer — list view
import type { User } from "../types/user";
import type { SortState } from "../types/query";
import { toggleSort } from "../types/query";
import { sortIcons } from "../constants/ui";

interface UserTableProps {
  users: User[];
  sort: SortState | null;
  onSort: (sort: SortState | null) => void;
  onEdit: (user: User) => void;
  onDelete: (id: number) => void;
}

function SortIcon({ field, sort }: Readonly<{ field: string; sort: SortState | null }>) {
  if (sort?.field !== field) return <span className="sort-icon">{sortIcons.both}</span>;
  return <span className="sort-icon active">{sort.dir === 'ASC' ? sortIcons.asc : sortIcons.desc}</span>;
}

export function UserTable({ users, sort, onSort, onEdit, onDelete }: Readonly<UserTableProps>) {
  function handleSort(field: string) {
    onSort(toggleSort(sort, field));
  }

  if (users.length === 0) {
    return <p className="empty">No users. Add one using the form above.</p>;
  }

  return (
    <table className="table">
      <thead>
        <tr>
          <th>#</th>
          <th className="sortable" onClick={() => handleSort('name')}>
            Name <SortIcon field="name" sort={sort} />
          </th>
          <th className="sortable" onClick={() => handleSort('email')}>
            Email <SortIcon field="email" sort={sort} />
          </th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {users.map((user) => (
          <tr key={user.id}>
            <td>{user.id}</td>
            <td>{user.name}</td>
            <td>{user.email}</td>
            <td>
              <div className="row-actions">
                <button className="btn btn-small" onClick={() => onEdit(user)}>Edit</button>
                <button className="btn btn-small btn-danger" onClick={() => onDelete(user.id)}>Delete</button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
