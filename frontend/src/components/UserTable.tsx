// Presentation layer - list view
import { useTranslation } from "react-i18next";
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

function SortIcon({
  field,
  sort,
}: Readonly<{ field: string; sort: SortState | null }>) {
  if (sort?.field !== field)
    return <span className="sort-icon">{sortIcons.both}</span>;
  return (
    <span className="sort-icon active">
      {sort.dir === "ASC" ? sortIcons.asc : sortIcons.desc}
    </span>
  );
}

export function UserTable({
  users,
  sort,
  onSort,
  onEdit,
  onDelete,
}: Readonly<UserTableProps>) {
  const { t } = useTranslation();

  function handleSort(field: string) {
    onSort(toggleSort(sort, field));
  }

  if (users.length === 0) {
    return <p className="empty">{t("users.empty")}</p>;
  }

  return (
    <table className="table">
      <thead>
        <tr>
          <th>#</th>
          <th className="sortable" onClick={() => handleSort("name")}>
            {t("table.name")} <SortIcon field="name" sort={sort} />
          </th>
          <th className="sortable" onClick={() => handleSort("email")}>
            {t("table.email")} <SortIcon field="email" sort={sort} />
          </th>
          <th>{t("table.phone")}</th>
          <th>{t("table.reportChannel")}</th>
          <th>{t("table.actions")}</th>
        </tr>
      </thead>
      <tbody>
        {users.map((user) => (
          <tr key={user.id}>
            <td>{user.id}</td>
            <td>{user.name}</td>
            <td>{user.email}</td>
            <td>{user.phoneNumber ?? "-"}</td>
            <td>{t(`report.channel${user.reportChannel}`)}</td>
            <td>
              <div className="row-actions">
                <button className="btn btn-small" onClick={() => onEdit(user)}>
                  {t("common.edit")}
                </button>
                <button
                  className="btn btn-small btn-danger"
                  onClick={() => onDelete(user.id)}
                >
                  {t("common.delete")}
                </button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
