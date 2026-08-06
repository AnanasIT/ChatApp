namespace IUserServiceModel;

using ServiceResultModel;
using UserDTO;

public interface IUserService
{
    Task<ServiceResult<UserDto>?> GetUserByIdAsync(int userId);
    Task<ServiceResult<UserDto>?> GetUserByUsernameAsync(string username);
    Task<ServiceResult<List<UserDto>>> GetAllUserAsync();
    Task<ServiceResult<bool>> UpdateUserRoleAsync(int userId, string newRole);
    Task<ServiceResult<bool>> DeleteUserAsync(int userId);
    Task<ServiceResult<UserDto>?> GetProfileAsync(int userId);
}