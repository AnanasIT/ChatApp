using ServiceResultModel;
using DirectChatRoomModel;
using DirectMessageModel;

using DirectMessageDTO;
using DirectChatRoomDTO;

namespace IDirectMessageServiceModel;

public interface IDirectMessageService
{
    Task<ServiceResult<List<DirectChatRoomDto>>> GetAllChatRoomsAsync(int userId);
    Task<ServiceResult<List<DirectMessageDto>>> GetMessagesAsync(int userId, int otherUserId, int limit = 50);
    Task<ServiceResult<DirectMessageDto>> SendMessageAsync(int senderId, int receiverId, string content);
    Task<ServiceResult<bool>> MarkAsReadAsync(int userId, int otherUserId);
    Task<ServiceResult<bool>> DeleteMessageAsync(int messageId, int userId);
}
