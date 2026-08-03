using ServiceResultModel;
using MessageDTO;

namespace IMessageServcieDTO;

public interface IMessageService
{
    Task<ServiceResult<MessageDto>> SaveMessageAsync(string username, int userId, 
                                                     string roomName, string content);
    Task<ServiceResult<List<MessageDto>>> GetHistoryAsync(string roomName, int count = 50);
}