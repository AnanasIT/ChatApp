using AdminUserDTO;
using AdmnRoomDTO;
using RoomStatsDTO;
using ChatStatDTO;

using ServiceResultModel;

namespace IAdminServiceModel;

public interface IAdminService
{
    Task<ServiceResult<List<AdminUserDto>>> GetAllUserAsync();
    Task<ServiceResult<AdminUserDto>> GetUserById(int userId);
    Task<ServiceResult<bool>> ChangeUserRoleAsync(int userId, string newRole);
    Task<ServiceResult<bool>> DeleteUserAsync(int userId);


    Task<ServiceResult<List<AdminRoomDto>>> GetAllRoomsAsync();
    Task<ServiceResult<RoomStatsDto>> GetRoomStatAsync(string roomName);
    Task<ServiceResult<bool>> DeleteRoomAsync(string roomName);
    Task<ServiceResult<bool>> ClearRoomHistoryAsync(string roomName);


    Task<ServiceResult<ChatStatsDto>> GetChatStatsAsync();
}