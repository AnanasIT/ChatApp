using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

using IJwtServiceModel;

using UserModel;

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
        
    }
}