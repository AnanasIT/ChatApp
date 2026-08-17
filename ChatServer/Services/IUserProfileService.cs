using UserProfileDTO;
using UserModel;
using ServiceResultModel;

namespace IUserProfileServiceModel;

public interface IUserProfileService
{
    Task<ServiceResult<UserProfileDto>> GetProfileAsync(int userId);
    Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task<ServiceResult<string>> UploadAvatarAsync(int userId, Stream fileStream, string fileName);
    Task<ServiceResult<bool>> DeleteAvatarAsync(int userId);
    Task<ServiceResult<List<UserProfileDto>>> GetUsersAsync(string? searchterm = null);
}