using IMessageServcieModel;
using AppDb;
using UserModel;
using RegisterDTO;
using MessageDTO;
using MessageModel;
using ServiceResultModel;
using RoomModel;

using Microsoft.EntityFrameworkCore; 

namespace MessageServiceModel;

public class MessageService : IMessageService
{
    public ILogger<MessageService> _logger;
    public AppDbContext _db;

    public MessageService (ILogger<MessageService> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<ServiceResult<MessageDto>> SaveMessageAsync(string username, int userId, 
                                                     string roomName, string content)
    {
        _logger.LogInformation($"{username} -> {roomName}: {content}");
        var room = await _db.Rooms.FirstOrDefaultAsync(t => t.Name == roomName);
        
        if (room is null)
        {
            _logger.LogWarning($"Комната {roomName} не найдена. Создаем комнату...");
            room = new Room {Name = roomName, CreatedAt = DateTime.UtcNow};
            await _db.Rooms.AddAsync(room);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Комната {roomName} создана!");
        }

        var message = new Message
        {
          Content = content,
          UserId = userId,
          RoomId = room.Id,
          SentAt = DateTime.UtcNow  
        };

        await _db.Messages.AddAsync(message);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Добавили сообщение в базу данных!");

        return ServiceResult<MessageDto>.Success(new MessageDto
        {
           Id = message.Id,
           UserName = username,
           Content = content,
           SentAt = message.SentAt 
        });
    }


    public async Task<ServiceResult<List<MessageDto>>> GetHistoryAsync(string roomName, int count = 50)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(t => t.Name == roomName);
        if (room is null) return ServiceResult<List<MessageDto>>.Success(null);

        var result = await _db.Messages
            .Include(m => m.User)
            .Where(m => m.RoomId == room.Id)
            .OrderByDescending(m => m.SentAt)
            .Take(count)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                UserName = m.User.UserName,
                Content = m.Content,
                SentAt = m.SentAt
            }).ToListAsync();
        
        return ServiceResult<List<MessageDto>>.Success(result);
    }
}