using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;

using AppDb;
using ChatHubs;
using IAuthServiceModel;
using ICacheModel;
using IJwtServiceModel;
using IMessageServcieModel;
using IRoomServiceModel;
using IUserServiceModel;

using AuthResponseDTO;
using LoginDTO;
using MessageDTO;
using RegisterDTO;
using UserDTO;

using ValidatorModel;
using JwtServiceModel;
using AuthSercviceModel;
using MessageServiceModel;
using RoomServiceModel;
using UserServiceModel;


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

builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<MessageValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RoomValidator>();

builder.Services.AddSignalR();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();


// ======================== ENDPOINTS ==========================