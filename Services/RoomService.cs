using Microsoft.EntityFrameworkCore;

using AppDb;
using RoomModel;
using IRoomServiceModel;
using ServiceResultModel;

namespace RoomServiceModel;

public class RoomService : IRoomService
{
    public readonly AppDbContext _db;
    public readonly ILogger<RoomService> _logger;

    public RoomService(AppDbContext db, ILogger<RoomService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ServiceResult<Room>> GetOrCreateRoomAsync(string roomName)
    {
        _logger.LogInformation($"Запрос на получение/создание комнаты {roomName}");
        var findRoom = await _db.Rooms.FirstOrDefaultAsync(t => t.Name == roomName);

        if (findRoom is null)
        {
            _logger.LogWarning($"Комната {roomName} не найдена! Создаем комнату...");
            var newRoom = new Room
            {
                Name = roomName,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Rooms.AddAsync(newRoom);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Создана комната {roomName}");
        }

        return ServiceResult<Room>.Success(findRoom!);
    }


    public async Task<ServiceResult<bool>> RoomExistsAsync(string roomName)
    {
        _logger.LogInformation($"Запрос на проверку существования комнаты {roomName}");
        return ServiceResult<bool>.Success(await _db.Rooms.AnyAsync(r => r.Name == roomName));
    }
}