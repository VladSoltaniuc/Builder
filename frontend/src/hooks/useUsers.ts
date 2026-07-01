// Service layer - users
import { useCallback, useEffect, useState } from 'react';
import { usersApi } from '../api/users';
import { ApiError } from '../api/errors';
import type { User, UserInput } from '../types/user';
import type { SortState } from '../types/query';
import { toSortBy } from '../types/query';
import { DEFAULT_PAGE_SIZE } from '../constants/pagination';

export function useUsers() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [totalCount, setTotalCount] = useState(0);

  // --- Sorting ---
  const [sort, setSort] = useState<SortState | null>(null);

  // --- Search ---
  const [search, setSearch] = useState('');

  const loadUsers = useCallback(async function loadUsers(
    p: number,
    ps: number,
    s: SortState | null,
    search: string,
  ) {
    setIsLoading(true);
    setError(null);
    try {
      const data = await usersApi.getAll(p, ps, toSortBy(s), search || undefined);
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
    if (page === 1) void loadUsers(1, pageSize, sort, search);
    else setPage(1);
  }, [loadUsers, page, pageSize, sort, search]);

  const updateUser = useCallback(async function updateUser(id: number, input: UserInput, version: number) {
    const updated = await usersApi.update(id, input, version);
    setUsers((prev) => prev.map((u) => (u.id === id ? updated : u)));
  }, []);

  const deleteUser = useCallback(async function deleteUser(id: number) {
    await usersApi.remove(id);
    const newPage = users.length === 1 && page > 1 ? page - 1 : page;
    if (newPage === page) void loadUsers(page, pageSize, sort, search);
    else setPage(newPage);
  }, [loadUsers, page, pageSize, users.length, sort, search]);

  useEffect(() => {
    void loadUsers(page, pageSize, sort, search);
  }, [loadUsers, page, pageSize, sort, search]);

  return {
    users, isLoading, error,
    page, totalPages: Math.max(1, Math.ceil(totalCount / pageSize)), setPage,
    pageSize, setPageSize,
    sort, setSort,
    search, setSearch,
    createUser, updateUser, deleteUser,
  };
}
