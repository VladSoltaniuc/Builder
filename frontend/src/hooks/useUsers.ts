// Service layer — users
import { useCallback, useEffect, useState } from 'react';
import { usersApi } from '../api/users';
import { ApiError } from '../api/errors';
import type { User, UserInput } from '../types/user';
import { PAGE_SIZE } from '../constants/pagination';

export function useUsers() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const loadUsers = useCallback(async function loadUsers(p: number) {
    setIsLoading(true);
    setError(null);
    try {
      const data = await usersApi.getAll(p, PAGE_SIZE);
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
    if (page === 1) void loadUsers(1);
    else setPage(1);
  }, [loadUsers, page]);

  const updateUser = useCallback(async function updateUser(id: number, input: UserInput, version: number) {
    const updated = await usersApi.update(id, input, version);
    setUsers((prev) => prev.map((u) => (u.id === id ? updated : u)));
  }, []);

  const deleteUser = useCallback(async function deleteUser(id: number) {
    await usersApi.remove(id);
    const newPage = users.length === 1 && page > 1 ? page - 1 : page;
    if (newPage === page) void loadUsers(page);
    else setPage(newPage);
  }, [loadUsers, page, users.length]);

  useEffect(() => {
    void loadUsers(page);
  }, [loadUsers, page]);

  return {
    users,
    isLoading,
    error,
    page,
    totalPages: Math.max(1, Math.ceil(totalCount / PAGE_SIZE)),
    setPage,
    createUser,
    updateUser,
    deleteUser,
  };
}
