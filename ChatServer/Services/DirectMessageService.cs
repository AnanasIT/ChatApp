using IDirectMessageServiceModel;
using AppDb;
using ServiceResultModel;
using DirectChatRoomModel;
using DirectMessageDTO;
using DirectChatRoomDTO;
using Microsoft.EntityFrameworkCore;
using DirectMessageModel;
using RoomModel;
using MessageModel;

namespace DirectMessageServiceModel;

public class DirectMessageService : IDirectMesasgeService
{
    protected readonly AppDbContext _db;
    protected readonly ILogger<DirectMessageService> _logger;

    public DirectMessageService(AppDbContext db, ILogger<DirectMessageService> logger) {
        _db = db;
        _logger = logger;
    }

    public async Task<ServiceResult<List<DirectChatRoomDto>>> GetAllChatRoomsAsync(int userId)
    {
        _logger.LogInformation($"📜 Запрос на получение всех комнат с участием пользователя {userId}");

        var rooms = await _db.DirectRooms.Where(t => t.UserIdOne == userId || t.UserIdTwo == userId)
                                          .ToListAsync();
        
        var result =new List<DirectChatRoomDto>();

        foreach (var item in rooms)
        {
            var otherUserId = item.UserIdOne == userId ? item.UserIdTwo : item.UserIdOne;
            var otherUser = await _db.Users.FindAsync(otherUserId);

            var lastMessage = await _db.DirectMessages
                                    .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || 
                                                (m.SenderId == otherUserId && m.ReceiverId == userId))
                                    .OrderByDescending(m => m.SentAt)
                                    .FirstOrDefaultAsync();
            
            var unreadCount = await _db.DirectMessages
                                    .CountAsync(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead);

            result.Add(new DirectChatRoomDto
            {
               Id = item.Id,
               OtherUserId = otherUserId,
               OtherUserName = otherUser?.UserName ?? "Неизвестно",
               LastMessage = lastMessage?.Content ?? "Нет сообщений",
               LastMessageAt = lastMessage?.SentAt ?? item.CreatedAt,
               UnreadCount = unreadCount 
            });   
        }

        return ServiceResult<List<DirectChatRoomDto>>.Success(result.OrderByDescending(r => r.LastMessage).ToList());
    }


    public async Task<ServiceResult<List<DirectMessageDto>>> GetMessagesAsync(int userId, int otherUserId, int limit = 50)
    {
        _logger.LogInformation($"📜 Запрос на получение личных собщений пользователей {userId} и {otherUserId}");
        var unreadMessages = await _db.DirectMessages
                                    .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
                                    .ToListAsync();
        
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        var messages = await _db.DirectMessages
                            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || 
                                  (m.SenderId == otherUserId && m.ReceiverId == userId))
                            .OrderByDescending(m => m.SentAt)
                            .Take(limit)
                            .OrderBy(m => m.SentAt)
                            .Select(m => new DirectMessageDto
                            {
                                Id = m.Id,
                                SenderId = m.SenderId,
                                SenderName = m.Sender.UserName,
                                ReceiverId = m.ReceiverId,
                                ReceiverName = m.Receiver.UserName,
                                Content = m.Content,
                                SentAt = m.SentAt,
                                IsRead = m.IsRead
                            }).ToListAsync();

        return ServiceResult<List<DirectMessageDto>>.Success(messages);
    }


    public async Task<ServiceResult<DirectMessageDto>> SendMessageAsync(int senderId, int receiverId, string content)
    {
        _logger.LogInformation($"📜 Запрос на личное сообщение от {senderId} к {receiverId}");

        var room = await _db.DirectRooms.FirstOrDefaultAsync(r => (r.UserIdOne == senderId && r.UserIdTwo == receiverId) || 
                                                                  (r.UserIdOne == receiverId && r.UserIdTwo == senderId));

        if (room is null) {
            room = new DirectChatRoom {
                UserIdOne = senderId,
                UserIdTwo = receiverId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.DirectRooms.AddAsync(room);
            await _db.SaveChangesAsync();
        }

        var message = new DirectMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        await _db.DirectMessages.AddAsync(message);
        room.LastMessageAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation($"📜 Личное сообщение от {senderId} к {receiverId}");

        var result = new DirectMessageDto {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = (await _db.Users.FindAsync(senderId))?.UserName ?? "Неизвестно",
            ReceiverId = message.ReceiverId,
            ReceiverName = (await _db.Users.FindAsync(receiverId))?.UserName ?? "Неизвестно",
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };

        return ServiceResult<DirectMessageDto>.Success(result); 
    }


    public async Task<ServiceResult<bool>> MarkAsReadAsync(int userId, int otherUserId)
    {
        var messages = await _db.DirectMessages
                            .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
                            .ToListAsync();
        
        foreach (var msg in messages) {
            msg.IsRead = true;
            msg.ReadAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }


    public async Task<ServiceResult<bool>> DeleteMessageAsync(int messageId, int userId)
    {
        var message = await _db.DirectMessages.FindAsync(messageId);
        if (message is null || message.SenderId == userId) {return ServiceResult<bool>.Success(false);}

        message.IsDeleted = true;
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }
}