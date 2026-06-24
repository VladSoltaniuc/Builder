// API layer — users
import { httpCore } from './httpCore';
import type { User, UserInput } from '../types/user';
import type { PagedResponse } from '../types/pagination';

const RESOURCE = '/users';

export const usersApi = {
  getAll: (page: number, pageSize: number) =>
    httpCore.get<PagedResponse<User>>(`${RESOURCE}?page=${page}&pageSize=${pageSize}`),

  getById: (id: number) =>
    httpCore.get<User>(`${RESOURCE}/${id}`),

  create: (input: UserInput) =>
    httpCore.post<User>(RESOURCE, input),

  update: (id: number, input: UserInput, version: number) =>
    httpCore.put<User>(`${RESOURCE}/${id}`, { ...input, version }),

  remove: (id: number) =>
    httpCore.delete(`${RESOURCE}/${id}`),
};
