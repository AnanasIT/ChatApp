using Microsoft.EntityFrameworkCore;

using AppDb;
using UserModel;
using RoomModel;
using MessageDTO;
using IRoomServiceModel;
using ServiceResultModel;

namespace RoomServiceModel;

public class RoomService : IRoomService
{
    private readonly AppDbContext _db;
    private readonly ILogger<RoomService> _logger;

    public RoomService(AppDbContext db, ILogger<RoomService> logger)
    {
        _db = db;
        _logger = logger;
    }
    
    public async Task<ServiceResult<Room>> GetOrCreateRoomAsync(string roomName)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Name == roomName);
        _logger.LogInformation($"Запрос на получение комнаты {roomName}");
        
        if (room is not null)
            return ServiceResult<Room>.Success(room);

        room = new Room
        {
            Name = roomName,
            CreatedAt = DateTime.UtcNow
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"🏠 Создана комната: {roomName}");
        return ServiceResult<Room>.Success(room);
    }

    public async Task<ServiceResult<bool>> RoomExistsAsync(string roomName)
    {
        _logger.LogInformation($"Запрос на проверку существования комнаты {roomName}");
        var result = await _db.Rooms.AnyAsync(r => r.Name == roomName);
        return ServiceResult<bool>.Success(result);
    }
}