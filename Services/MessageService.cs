using IMessageServcieModel;
using AppDb;
using UserModel;
using RegisterDTO;
using MessageDTO;
using MessageModel;
using ServiceResultModel;
using RoomModel;
using ICacheModel;

using Microsoft.EntityFrameworkCore; 

namespace MessageServiceModel;

public class MessageService : IMessageService
{
    public readonly ILogger<MessageService> _logger;
    public readonly AppDbContext _db;
    public readonly ICacheService _cache;


    public MessageService (ILogger<MessageService> logger, AppDbContext db, ICacheService cache)
    {
        _logger = logger;
        _db = db;
        _cache = cache;
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

        var cacheKey = $"history_{roomName}_50";
        _cache.Remove(cacheKey);
        _logger.LogInformation($"Кэш для {roomName} очищен!");

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
        var cache_key = $"history_{roomName}_{count}";
        
        if (_cache.TryGet<List<MessageDto>>(cache_key, out var cached))
        {
            _logger.LogInformation($"История для {roomName} загружена из кэша!");
            return ServiceResult<List<MessageDto>>.Success(cached!);    
        }

        _logger.LogInformation($"История для {roomName} загружается из БД...");

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

        _cache.Set(cache_key, result, TimeSpan.FromMinutes(2));
        return ServiceResult<List<MessageDto>>.Success(result);
    }
}