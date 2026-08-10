using ServiceResultModel;
using MessageDTO;

namespace IMessageServcieDTO;

public interface IMessageService
{
    Task<ServiceResult<MessageDto>> SaveMessageAsync(string username, int userId, 
                                                     string roomName, string content);
    Task<ServiceResult<List<MessageDto>>> GetHistoryAsync(string roomName, int count = 50);
    Task<ServiceResult<bool>> DeleteMessageAsync(int messageId, int userId, bool isAdmin);
    Task<ServiceResult<bool>> EditMessageAsync(int messageId, int userId, string newContent);
}