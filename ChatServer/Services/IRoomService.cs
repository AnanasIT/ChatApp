using ServiceResultModel;
using RoomModel;

namespace IRoomServiceModel;

public interface IRoomService
{
    Task<ServiceResult<Room>> GetOrCreateRoomAsync(string roomName);
    Task<ServiceResult<bool>> RoomExistsAsync(string roomName);
}