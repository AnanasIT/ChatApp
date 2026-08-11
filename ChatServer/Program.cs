using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using FluentValidation;

using AppDb;
using ChatHubs;
using IAuthServiceModel;
using ICache;
using IJwtServiceModel;
using IMessageServcieDTO;
using IRoomServiceModel;
using IUserServiceModel;

using LoginDTO;
using RegisterDTO;

using ValidatorModel;
using JwtServiceModel;
using AuthSercviceModel;
using MessageServiceModel;
using RoomServiceModel;
using UserServiceModel;

namespace ChatServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=chat.db"));

        var secretKey = builder.Configuration["JwtSettings:SecretKey"] ?? throw new Exception("JwtSettings:SecretKey не найден в User Secrets!");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Ключ загружен! Длина {secretKey.Length}");
        Console.ResetColor();

        var key = Encoding.UTF8.GetBytes(secretKey);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/chat"))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<ICacheService, CacheService>();

        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddScoped<IRoomService, RoomService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<MessageValidator>();
        builder.Services.AddScoped<RoomValidator>();
        builder.Services.AddScoped<IValidator<RegisterRequestUser>,RegisterRequestValidator>();
        builder.Services.AddScoped<IValidator<LoginRequestUser>, LoginRequestValidator>();

        builder.Services.AddSignalR();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStaticFiles();

        // ===== PUBLIC ENDPOINTS =====
        app.MapGet("/", () => "Сервер запущен. Присоединяйся к чату через C# клиент!");

        app.MapPost("/register", async (RegisterRequestUser request, IAuthService service, IValidator<RegisterRequestUser> validator) =>
        {
            var valid = await validator.ValidateAsync(request);

            if (!valid.IsValid)
            {
                var errors = valid.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(new { errors });
            }

            var result = await service.RegisterAsync(request)!;
            return result.IsSucces ? Results.Ok(result) : Results.BadRequest(new { error = result.Error });
        });

        app.MapPost("/login", async (LoginRequestUser request, IAuthService service, IValidator<LoginRequestUser> validator) =>
        {
            var valid = await validator.ValidateAsync(request);

            if (!valid.IsValid)
            {
                var errors = valid.Errors.Select(e => e.ErrorMessage);
                return Results.BadRequest(new { errors });
            }

            var result = await service.LoginAsync(request)!;
            return result.IsSucces ? Results.Ok(result) : Results.BadRequest(new { error = result.Error });
        });

        // ===== SIGNALR HUB =====
        app.MapHub<ChatHub>("/chat");

        // ===== PRIVATE ENDPOINTS =====
        app.MapGet("/profile", async (HttpContext context, IUserService service) =>
        {
            var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await service.GetProfileAsync(userId);
            return result!.IsSucces ? Results.Ok(result) : Results.BadRequest(new { error = result.Error });
        })
        .RequireAuthorization();

        // ===== START =====
        app.Run();
    }
}