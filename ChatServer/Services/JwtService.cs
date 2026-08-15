using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

using IJwtServiceModel;

using UserModel;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace JwtServiceModel;

public class JwtService : IJwtService
{
    public IConfiguration? _config;
    public ILogger<JwtService>? _logger;

    public JwtService(IConfiguration config, ILogger<JwtService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string GenerateToken(User User)
    {
        _logger.LogInformation($"Генерация токена для {User.UserName}...");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, User.Id.ToString()),
            new Claim(ClaimTypes.Name, User.UserName),
            new Claim(ClaimTypes.Role, User.Role)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires:DateTime.UtcNow.AddMinutes(60),
            signingCredentials:credentials
        );

        _logger.LogInformation($"Токен для {User.UserName} (Role: {User.Role}) сгенерирован!");
        _logger.LogInformation($"Токен: {new JwtSecurityTokenHandler().WriteToken(token)}");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}