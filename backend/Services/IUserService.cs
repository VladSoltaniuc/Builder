// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IUserService
{
    Task<PagedResponse<UserResponse>> GetAll(PageQuery query);
    Task<UserResponse?> GetById(int id);
    Task<UserResponse> Create(CreateUserRequest request);
    Task<UpdateUserResult> Update(int id, UpdateUserRequest request);
    Task<bool> Delete(int id);
}
