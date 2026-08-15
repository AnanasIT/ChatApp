using AppDb;
using IAdminServiceModel;
using ServiceResultModel;
using AdminUserDTO;
using AdmnRoomDTOModel;
using RoomStatsDTO;
using ChatStatDTO;
using Microsoft.EntityFrameworkCore;

namespace AdminServiceModel;

public class AdminService : IAdminService
{
    protected readonly ILogger<AdminService> _logger;
    protected readonly AppDbContext _db;

    public AdminService (ILogger<AdminService> logger, AppDbContext db) {
        _logger = logger;
        _db = db;
    }


    public async Task<ServiceResult<List<AdminUserDto>>> GetAllUserAsync()
    {
        _logger.LogInformation("📋 Запрос от админа на получение всех пользователей 📋");
        List<AdminUserDto> result = await _db.Users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            MessageCount = _db.Messages.Count(M => M.UserId == u.Id)
        }).ToListAsync();

        return ServiceResult<List<AdminUserDto>>.Success(result);
    }


    public async Task<ServiceResult<AdminUserDto>> GetUserById(int userId)
    {
        _logger.LogInformation($"📋 Запрос от админа на получение пользователя по ID {userId} 📋");
        var findUser = await _db.Users.FindAsync(userId);

        if (findUser is null) {
            _logger.LogWarning($"⚠️ Пользователь с ID {userId} не найден! ⚠️");
            return ServiceResult<AdminUserDto>.Failure($"Пользователь с ID {userId} не найден!");
        }

        return ServiceResult<AdminUserDto>.Success(new AdminUserDto
        {
           Id = findUser.Id,
           UserName = findUser.UserName,
           Email = findUser.Email,
           Role = findUser.Role,
           CreatedAt = findUser.CreatedAt,
           MessageCount = _db.Messages.Count(m => m.UserId == userId)
        });
    }


    public async Task<ServiceResult<bool>> ChangeUserRoleAsync(int userId, string newRole)
    {
        _logger.LogInformation($"📋 Запрос от админа на изменение роли пользователя {userId} 📋 ");

        var findUser = await _db.Users.FindAsync(userId);
        if (findUser is null) {
            _logger.LogWarning($"⚠️ Пользователь с ID {userId} не найден! ⚠️");
            return ServiceResult<bool>.Failure($"Пользователь с ID {userId} не найден!");
        }

        findUser.Role = newRole;
        await _db.SaveChangesAsync();

        _logger.LogInformation($"✅ Роль пользователя {userId} успешно изменена!");
        return ServiceResult<bool>.Success(true);
    }


    public async Task<ServiceResult<bool>> DeleteUserAsync(int userId)
    {
        _logger.LogInformation($"📋 Запрос от админа на удаление пользователя {userId} 📋");

        var findUser = await _db.Users.FindAsync(userId);
        if (findUser is null) {
            _logger.LogWarning($"⚠️ Пользователь с ID {userId} не найден! ⚠️");
            return ServiceResult<bool>.Failure($"Пользователь с ID {userId} не найден!");
        }

        var messages = await _db.Messages.Where(m => m.UserId == userId).ToListAsync();
        _db.Messages.RemoveRange(messages);

        _db.Users.Remove(findUser);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation($"✅ Пользователь успешно удален!");
        return ServiceResult<bool>.Success(true);
    }


    public async Task<ServiceResult<List<AdminRoomDto>>> GetAllRoomsAsync()
    {
        _logger.LogInformation("📋 Запрос от админа на получение всех комнат 📋");
        
        List<AdminRoomDto> allRooms = await _db.Rooms.Select(r => new AdminRoomDto
        {
            Id = r.Id,
            Name = r.Name,
            CreatedAt = r.CreatedAt,
            MessageCount = _db.Messages.Count(m => m.RoomId ==r.Id)
        }).ToListAsync();

        return ServiceResult<List<AdminRoomDto>>.Success(allRooms);
    }


    public async Task<ServiceResult<RoomStatsDto>> GetRoomStatAsync(string roomName)
    {
        _logger.LogInformation($"📋 Запрос от админа на получение статистики комнаты {roomName} 📋");
        
        var room = await _db.Rooms.FirstOrDefaultAsync(t => t.Name == roomName);
        if (room is null) {
            _logger.LogWarning($"⚠️ Комната {roomName} не найдена! ⚠️");
            return ServiceResult<RoomStatsDto>.Failure($"Комната {roomName} не найдена!");
        }

        var messages = await _db.Messages.Where(m => m.RoomId == room.Id).ToListAsync();
        var users = messages.Select(m => m.User.UserName).Distinct().ToList();

        return ServiceResult<RoomStatsDto>.Success(new RoomStatsDto
        {
           RoomName = room.Name,
           TotalMessages = messages.Count,
           UniqueUsers = users.Count,
           Users = users 
        });
    }


    public async Task<ServiceResult<bool>> DeleteRoomAsync(string roomName)
    {
        _logger.LogInformation($"📋 Запрос от админа на удаление комнаты {roomName} 📋");

        var findRoom = await _db.Rooms.FirstOrDefaultAsync(t => t.Name == roomName);
        if (findRoom is null) {
            _logger.LogWarning($"⚠️ Комната {roomName} не найдена! ⚠️");
            return ServiceResult<bool>.Success(false);
        }

        _db.Rooms.Remove(findRoom);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"✅ Комната {roomName} успешно удалена!");
        return ServiceResult<bool>.Success(true);
    }


    public async Task<ServiceResult<bool>> ClearRoomHistoryAsync(string roomName)
    {
        _logger.LogInformation($"📋 Запрос от админа на очистку истории комнаты {roomName} 📋");
        
        var findRoom = await _db.Rooms.FirstOrDefaultAsync(t => t.Name == roomName);
        if (findRoom is null) {
            _logger.LogWarning($"⚠️ Комната {roomName} не найдена! ⚠️");
            return ServiceResult<bool>.Success(false);
        }

        var messages = await _db.Messages.Where(m => m.RoomId == findRoom.Id).ToListAsync();

        _db.Messages.RemoveRange(messages);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"История комнаты {roomName} очищена!");
        return ServiceResult<bool>.Success(true);
    }


    public async Task<ServiceResult<ChatStatsDto>> GetChatStatsAsync()
    {
        _logger.LogInformation($"📋 Запрос от админа на получение статистика чата 📋");

        var totalUsers = await _db.Users.CountAsync();
        var totalRooms = await _db.Rooms.CountAsync();
        var totalMessages = await _db.Messages.CountAsync();
        var todayMessages = await _db.Messages
            .CountAsync(m => m.SentAt.Date == DateTime.UtcNow.Date);
        
        return ServiceResult<ChatStatsDto>.Success(new ChatStatsDto
        {
           TotalUsers = totalUsers,
           TotalRooms = totalRooms,
           TotalMessages = totalMessages,
           TodayMessages = todayMessages 
        });
    }
}