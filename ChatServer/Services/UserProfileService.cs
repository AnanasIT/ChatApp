using UserModel;
using UserProfileDTO;
using IUserProfileServiceModel;
using ServiceResultModel;
using AppDb;
using Microsoft.EntityFrameworkCore;

namespace UserProfileServiceModel;

public class UserProfileService : IUserProfileService
{
    protected readonly ILogger<UserProfileService> _logger;
    protected readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public UserProfileService(ILogger<UserProfileService> logger, AppDbContext db, 
                            IWebHostEnvironment env) {
        _logger = logger;
        _db = db;
        _env = env;
    }


    public async Task<ServiceResult<UserProfileDto>> GetProfileAsync(int userId)
    {
        _logger.LogInformation($"👤 Запрос на получение профиля {userId}");

        var findUser = await _db.Users.FindAsync(userId);
        if (findUser is null) {
            _logger.LogWarning($"Пользователь с ID {userId} не найден!");
            return ServiceResult<UserProfileDto>.Failure($"⚠️ Пользователь с ID {userId} не найден!");
        }

        return ServiceResult<UserProfileDto>.Success(new UserProfileDto{
            Id = findUser.Id,
            UserName = findUser.UserName,
            Bio = findUser.Bio ?? "Unknown",
            AvatarURL = findUser.AvatarPath ?? "None",
            Role = findUser.Role
        });
    }


    public async Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        _logger.LogInformation($"👤 Запрос на обновление профиля {userId}");

        var findUser = await _db.Users.FindAsync(userId);
        if (findUser is null) {
            _logger.LogWarning($"Пользователь с ID {userId} не найден!");
            return ServiceResult<UserProfileDto>.Failure($"⚠️ Пользователь с ID {userId} не найден!");
        }

        if (!string.IsNullOrWhiteSpace(dto.UserName)) {
            var exists = await _db.Users.AnyAsync(u => u.UserName == dto.UserName);
            if (exists) {
                _logger.LogError($"❌ Пользователь с именем {dto.UserName} уже существует в бд!");
                return ServiceResult<UserProfileDto>.Failure($"Пользователь с именем {dto.UserName} уже существует в бд!");
            }
        }

        findUser.Bio = dto.Bio;
        findUser.UserName = dto.UserName;

        await _db.SaveChangesAsync();

        return ServiceResult<UserProfileDto>.Success(new UserProfileDto
        {
            Id = findUser.Id,
            UserName = findUser.UserName,
            Bio = findUser.Bio ?? "Unknown",
            AvatarURL = findUser.AvatarPath ?? "None",
            Role = findUser.Role
        });
    }


    public async Task<ServiceResult<string>> UploadAvatarAsync(int userId, Stream fileStream, string fileName)
    {
        _logger.LogInformation($"👤🖼 Запрос на обновление аватарки {userId}");

        var findUser = await _db.Users.FindAsync(userId);
        if (findUser is null) {
            _logger.LogWarning($"Пользователь с ID {userId} не найден!");
            return ServiceResult<string>.Failure($"⚠️ Пользователь с ID {userId} не найден!");
        }

        if (!string.IsNullOrWhiteSpace(findUser.AvatarPath)) {
            var oldPath = Path.Combine(_env.WebRootPath, "avatars");
            if (File.Exists(oldPath))
                File.Delete(oldPath);
        }

        // Создание папки
        var avatarsDir = Path.GetExtension(fileName);
        if (!Directory.Exists(avatarsDir))
            Directory.CreateDirectory(avatarsDir);
        
        // Генерация имени
        var extensions = Path.GetExtension(fileName);
        var newFileName = $"{userId}_{DateTime.Now.Ticks}{extensions}";
        var filePath = Path.Combine(avatarsDir, newFileName);

        // Сохраняем
        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        var avatarUrl = $"/avatars/{newFileName}";
        findUser.AvatarPath = avatarUrl;
        await _db.SaveChangesAsync();

        return ServiceResult<string>.Success(avatarUrl);
    }


    public async Task<ServiceResult<bool>> DeleteAvatarAsync(int userId)
    {
        _logger.LogInformation($"🖼🗑️ Запрос на удаление аватарки пользователя {userId}");

        var findUser = await _db.Users.FindAsync(userId);
        if (findUser is null) {
            _logger.LogWarning($"Пользователь с ID {userId} не найден!");
            return ServiceResult<bool>.Failure($"⚠️ Пользователь с ID {userId} не найден!");
        }

        if (!string.IsNullOrWhiteSpace(findUser.AvatarPath)) {
            var filePath = Path.Combine(_env.WebRootPath, findUser.AvatarPath.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);
            
            findUser.AvatarPath = null;
            await _db.SaveChangesAsync();
        }

        return ServiceResult<bool>.Success(true);
    }


    public async Task<ServiceResult<List<UserProfileDto>>> GetUsersAsync(string? searchTerm = null)
    {
        _logger.LogInformation($"👥📊 Запрос на получение пользователей по фильтру {searchTerm}");
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var findUsers = await _db.Users.Where(u => u.UserName.Contains(searchTerm)).ToListAsync();
            var result = findUsers.Select(u => new UserProfileDto
            {
               Id = u.Id,
               UserName = u.UserName,
               Bio = u.Bio ?? "Unknown",
               AvatarURL = u.AvatarPath ?? "Unknown",
               Role = u.Role 
            }).ToList();

            return ServiceResult<List<UserProfileDto>>.Success(result);
        }

        else {
            _logger.LogWarning("⚠️ Ничего не найдено!");
            return ServiceResult<List<UserProfileDto>>.Failure("Ничего не найдено!");
        }
    }
}