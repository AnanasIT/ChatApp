using AppDb;
using UserModel;
using RegisterDTO;
using MessageDTO;
using MessageModel;
using ServiceResultModel;
using RoomModel;
using ICache;
using IMessageServcieDTO;
using SearchMessageModel;

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
          SentAt = DateTime.UtcNow,
          IsDeleted = false,
          IsEdited = false  
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
           SentAt = message.SentAt,
           IsDeleted = false,
           IsEdited = false 
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
        if (room is null) return ServiceResult<List<MessageDto>>.Success(null!);

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

        _logger.LogInformation($"Возвращаю {result.Count} сообщений!");
        return ServiceResult<List<MessageDto>>.Success(result);
    }


    public async Task<ServiceResult<bool>> DeleteMessageAsync(int messageId, int userId, bool isAdmin)
    {
        try
       { 
            _logger.LogInformation($"Запрос на удаление сообщения {messageId} пользователя {userId}");

            var message = await _db.Messages.FindAsync(messageId);
            if (message is null) {return ServiceResult<bool>.Success(false);}

            if (message.UserId != userId && !isAdmin){
                _logger.LogWarning("Запрос отклонен, так как доступно только для админа!");
                return ServiceResult<bool>.Success(false);
            }

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Сообщение {messageId} удалено!");
            return ServiceResult<bool>.Success(true);
       }

       catch (Exception ex)
       {
            return ServiceResult<bool>.Failure(ex.Message);
       }
    }


    public async Task<ServiceResult<bool>> EditMessageAsync(int messageId, int userId, string newContent)
    {
        try
        {
            _logger.LogInformation($"Запрос на редактирование сообщения {messageId} от пользователя {userId}");

            var message = await _db.Messages.FindAsync(messageId);
            if (message is null) {return ServiceResult<bool>.Success(false);}

            if (message.UserId != userId) {
                _logger.LogWarning("Запрос отклонен, так как редактирование доступно только автору сообщения!");
                return ServiceResult<bool>.Success(false);
            }

            message.Content = newContent;
            message.IsEdited = true;
            message.EditedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }

        catch(Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message);
        }
    }


    public async Task<ServiceResult<List<MessageDto>>> SearchMessagesAsync(SearchMessageDto request)
    {
        _logger.LogInformation($"🔎 Поиск в комнате {request.RoomName} по запросу {request.Query}");

        var findRoom = await _db.Rooms.FirstOrDefaultAsync(t => t.Name == request.RoomName);
        if (findRoom is null) {
            _logger.LogWarning($"❌ Комната {request.RoomName} не найдена!");
            return ServiceResult<List<MessageDto>>.Failure($"❌ Комната {request.RoomName} не найдена!");
        }

        var messages = await _db.Messages
            .Include(m => m.User)
            .Where(m => m.RoomId == findRoom.Id && m.Content.Contains(request.Query))
            .OrderByDescending(m => m.SentAt)
            .Take(request.Limit)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                UserName = m.User.UserName,
                Content = m.Content,
                SentAt = m.SentAt,
                IsDeleted = m.IsDeleted,
                IsEdited = m.IsEdited
            }).ToListAsync();
        
        _logger.LogInformation($"✅ найдено {messages.Count} сообщений!");
        return ServiceResult<List<MessageDto>>.Success(messages);
    }
}