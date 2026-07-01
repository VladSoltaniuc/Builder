// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IUserService
{
    Task<PagedResponse<UserResponse>> GetAll(PageQuery query);
    Task<UserResponse> GetById(int id);
    Task<UserResponse> Create(CreateUserRequest request);
    Task<UserResponse> Update(int id, UpdateUserRequest request);
    Task Delete(int id);
}
