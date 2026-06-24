// Service layer — users
import { useCallback, useEffect, useState } from 'react';
import { usersApi } from '../api/users';
import { ApiError } from '../api/errors';
import type { User, UserInput } from '../types/user';
import type { SortState } from '../types/query';
import { toSortBy } from '../types/query';
import { PAGE_SIZE } from '../constants/pagination';

export function useUsers() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // --- Sorting ---
  const [sort, setSort] = useState<SortState | null>(null);

  // --- Filtering ---
  const [search, setSearch] = useState('');
  const [filters, setFilters] = useState<Record<string, string>>({});

  const loadUsers = useCallback(async function loadUsers(
    p: number,
    s: SortState | null,
    search: string,
    filters: Record<string, string>,
  ) {
    setIsLoading(true);
    setError(null);
    try {
      const data = await usersApi.getAll(p, PAGE_SIZE, toSortBy(s), search || undefined, Object.keys(filters).length ? filters : undefined);
      setUsers(data.items);
      setTotalCount(data.totalCount);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : ApiError.fromStatus(1).message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const createUser = useCallback(async function createUser(input: UserInput) {
    await usersApi.create(input);
    if (page === 1) void loadUsers(1, sort, search, filters);
    else setPage(1);
  }, [loadUsers, page, sort, search, filters]);

  const updateUser = useCallback(async function updateUser(id: number, input: UserInput, version: number) {
    const updated = await usersApi.update(id, input, version);
    setUsers((prev) => prev.map((u) => (u.id === id ? updated : u)));
  }, []);

  const deleteUser = useCallback(async function deleteUser(id: number) {
    await usersApi.remove(id);
    const newPage = users.length === 1 && page > 1 ? page - 1 : page;
    if (newPage === page) void loadUsers(page, sort, search, filters);
    else setPage(newPage);
  }, [loadUsers, page, users.length, sort, search, filters]);

  useEffect(() => {
    void loadUsers(page, sort, search, filters);
  }, [loadUsers, page, sort, search, filters]);

  return {
    users, isLoading, error,
    page, totalPages: Math.max(1, Math.ceil(totalCount / PAGE_SIZE)), setPage,
    sort, setSort,
    search, setSearch,
    filters, setFilters,
    createUser, updateUser, deleteUser,
  };
}
