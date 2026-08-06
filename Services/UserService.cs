namespace UserServiceModel;

using ServiceResultModel;
using UserDTO;
using AppDb;

using Microsoft.EntityFrameworkCore;
using IUserServiceModel;

public class UserService : IUserService
{
    public readonly AppDbContext _db;
    public readonly ILogger<UserService> _logger;

    public UserService(AppDbContext db, ILogger<UserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ServiceResult<UserDto>?> GetUserByIdAsync(int userId)
    {
        _logger.LogInformation($"Запрос на получение пользователя по ID {userId}");

        var findUser = await _db.Users.FirstOrDefaultAsync(t => t.Id == userId);
        if (findUser is null){
            _logger.LogWarning($"Пользователя с ID {userId} не найдено!");
            return ServiceResult<UserDto>.Success(null);
        }

        _logger.LogInformation($"Пользователь по ID {userId} получен!");
        return ServiceResult<UserDto>.Success(new UserDto
        {
           Id = findUser.Id,
           UserName = findUser.UserName,
           Email = findUser.UserName,
           Role = findUser.Role,
           CreatedAt = findUser.CreatedAt 
        });
    }


    public async Task<ServiceResult<UserDto>?> GetUserByUsernameAsync(string username)
    {
        _logger.LogInformation($"Запрос на получение пользователя по имени {username}");
        
        var findUser = await _db.Users.FirstOrDefaultAsync(t => t.UserName == username);
        if (findUser is null){
            _logger.LogWarning($"Пользователя с именем {username} не найдено!");
            return ServiceResult<UserDto>.Success(null);
        }

        _logger.LogInformation($"Пользователь по имени {username} получен!");
        return ServiceResult<UserDto>.Success(new UserDto
        {
           Id = findUser.Id,
           UserName = findUser.UserName,
           Email = findUser.UserName,
           Role = findUser.Role,
           CreatedAt = findUser.CreatedAt 
        });
    }


    public async Task<ServiceResult<List<UserDto>>> GetAllUserAsync()
    {
        _logger.LogInformation("Запрос на получение всех пользователей!");
        
        var allUsers = await _db.Users.Select(user => new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        }).ToListAsync();

        _logger.LogInformation("Все пользователи получены!");
        return ServiceResult<List<UserDto>>.Success(allUsers);
    }


    public async Task<ServiceResult<bool>> UpdateUserRoleAsync(int userId, string newRole)
    {
        _logger.LogInformation($"Запрос на обновление роли для пользователя с ID {userId}");
        var findUser = await _db.Users.FirstOrDefaultAsync(t => t.Id == userId);

        if (findUser is null){
            _logger.LogWarning($"Пользователя с ID {userId} не найдено!");
            return ServiceResult<bool>.Success(false);
        }

        findUser.Role = newRole;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Роль успешно изменена!");
        return ServiceResult<bool>.Success(true);
    }


    public async Task<ServiceResult<bool>> DeleteUserAsync(int userId)
    {
        _logger.LogInformation($"Запрос на удаление пользователя по ID {userId}");
        var findUser = await _db.Users.FirstOrDefaultAsync(t => t.Id == userId);

        if (findUser is null){
            _logger.LogWarning($"Пользователя с ID {userId} не найдено!");
            return ServiceResult<bool>.Success(false);
        }

        _db.Users.Remove(findUser);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"Пользователь с ID {userId} удален!");
        return ServiceResult<bool>.Success(true);
    }


    public async Task<ServiceResult<UserDto>?> GetProfileAsync(int userId)
    {
        _logger.LogInformation($"Запрос профиля пользователя с ID {userId}");

        var user = await _db.Users.FirstOrDefaultAsync(t => t.Id == userId);
        
        if (user is null) {
            _logger.LogError($"Пользователь с ID {userId} не найден!");
            return null;
        }

        return ServiceResult<UserDto>.Success(new UserDto
        {
           Id = user.Id,
           UserName = user.UserName,
           Email = user.Email,
           Role = user.Role,
           CreatedAt = user.CreatedAt 
        });
    }

}