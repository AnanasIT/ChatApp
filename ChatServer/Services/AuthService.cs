namespace AuthSercviceModel;

using IAuthServiceModel;
using AppDb;
using UserModel;
using RegisterDTO;
using ServiceResultModel;
using AuthResponseDTO;
using Microsoft.EntityFrameworkCore;
using PasswordHelperModel;
using LoginDTO;

using IJwtServiceModel;

public class AuthService : IAuthService
{
    public ILogger<AuthService> _logger;
    public AppDbContext _db;
    public IConfiguration _config;
    public IJwtService _jwt;

    public AuthService(ILogger<AuthService> logger, AppDbContext db, IConfiguration config, IJwtService jwt) {
        _logger = logger;
        _db = db;
        _config = config;
        _jwt = jwt;
    }

    public async Task<ServiceResult<AuthResponse>>? RegisterAsync(RegisterRequestUser request)
    {
        _logger.LogInformation($"Запрос на регистрацию от пользователя {request.UserName}");

        var findUser = await _db.Users.FirstOrDefaultAsync(t => t.UserName == request.UserName);
        if (findUser is not null){
            _logger.LogError($"Пользователь с именем {request.UserName} уже существует в базе данных!");
            return ServiceResult<AuthResponse>.Failure($"Пользователь с именем {request.UserName} уже существует в базе данных!");
        }

        string Role = string.Empty;

        if (_config["Admin:Username"] == request.UserName && 
            _config["Admin:Password"] == request.Password) {
            Role = "Admin";
        }

        else
        {
            Role = "User";
        }

        var hashPassword = HashPassword.PasswordHash(request.Password);

        var newUser = new User
        {
            UserName = request.UserName,
            PasswordHash = hashPassword,
            Email = request.Email,
            Role = Role
        };

        await _db.Users.AddAsync(newUser);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"✅ {newUser.UserName} успешно зарегистрировался! Роль - {newUser.Role}");
        var token = _jwt.GenerateToken(newUser);

        return ServiceResult<AuthResponse>.Success(new AuthResponse
        {
           Token = token,
           UserName = newUser!.UserName,
           Role = newUser.Role
        });
    }



    public async Task<ServiceResult<AuthResponse>>? LoginAsync(LoginRequestUser request)
    {
        _logger.LogInformation($"Запрос от {request.UserName} на вход в аккаунт!");
        var findUser = await _db.Users.FirstOrDefaultAsync(t => t.UserName == request.UserName);

        if (findUser is null){
            _logger.LogError($"Пользователя с именем {request.UserName} не найдено в базе данных!");
            return ServiceResult<AuthResponse>.Failure($"Пользователя с именем {request.UserName} не найдено в базе данных!");
        }

        bool verifyPassword = HashPassword.VerifyPassword(request.Password, findUser.PasswordHash);
        if (verifyPassword is false){
            _logger.LogError($"Неправильный пароль!");
            return ServiceResult<AuthResponse>.Failure($"Неправильный пароль!");
        }

        _logger.LogInformation($"✅ Успешный вход в аккаунт {request.UserName} Роль - {findUser.Role}");
        var token = _jwt.GenerateToken(findUser);

        return ServiceResult<AuthResponse>.Success(new AuthResponse
        {
           Token = token,
           Role = findUser.Role,
           UserName = findUser.UserName 
        });
    }
}
