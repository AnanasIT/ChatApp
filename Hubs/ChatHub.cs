using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using IMessageServcieModel;
using IRoomServiceModel;


namespace ChatHubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly IRoomService _roomService;
    private ILogger<ChatHub> _logger;
    private static readonly Dictionary<string, string> _onlineUsers = new();

    public ChatHub(IMessageService messageService, IRoomService roomService, ILogger<ChatHub> logger){
        _messageService = messageService;
        _roomService = roomService;
        _logger = logger;
    }

    private string? GetUserName() => Context.User?.Identity?.Name;
    private int GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? int.Parse(claim.Value) : 0;
    }

    public override async Task OnConnectedAsync()
    {
        var userName = GetUserName() ?? "Аноним";
        var userId = GetUserId();

        _onlineUsers[Context.ConnectionId] = userName;
        _logger.LogInformation($"{userName} (ID: {userId}) подключился!");

        var history = await _messageService.GetHistoryAsync("Общий", 50);
        await Clients.Caller.SendAsync("ReceiveHistory", history);

        await Clients.Caller.SendAsync("ReceiveMessage", "Система", $"Добро пожаловать, {userName}!");
        await Clients.Others.SendAsync("ReceiveMessage", "Система", $"{userName} подключился!");

        await SendOnlineUsers();

        await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var userName = GetUserName() ?? "Аноним";
        var userId = GetUserId();

        _onlineUsers.Remove(Context.ConnectionId);
        _logger.LogInformation($"{userName} (ID: {userId}) отключился!");
        
        await Clients.All.SendAsync("ReceiveMessage", "Система", $"{userName} отключился!");

        await SendOnlineUsers();

        await base.OnDisconnectedAsync(ex);
    }


    public async Task SendMessageToRoom(string message, string roomName)
    {
        var userName = GetUserName() ?? "Аноним";
        var userId = GetUserId();

        if (string.IsNullOrEmpty(message)) return;
        if (string.IsNullOrEmpty(roomName)) roomName = "Общий";

        await _messageService.SaveMessageAsync(userName, userId, roomName, message);
        await Clients.Group(roomName).SendAsync("ReceiveMessage", userName, message);

        _logger.LogInformation($"[{roomName}] {userName} : {message}");
    }


    public async Task JoinRoom(string roomName)
    {
        var userName = GetUserName() ?? "Аноним";
        
        if (string.IsNullOrWhiteSpace(roomName)) roomName = "Общий";

        await _roomService.GetOrCreateRoomAsync(roomName);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("ReceiveMessage", "Система", $"{userName} присоединился к комнате '{roomName}'");

        _logger.LogInformation($"{userName} присоединился к комнате '{roomName}'");
    }


    public async Task LeaveRoom(string roomName)
    {
        var userName = GetUserName() ?? "Аноним";

        if (string.IsNullOrWhiteSpace(roomName)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("ReceiveMessage", "Система", $"{userName} покинул группу {roomName}");
    }


    public async Task SendPrivateMessage(string toUser, string message)
    {
        var fromUser = GetUserName() ?? "Аноним";
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(message)) return;
        if (string.IsNullOrWhiteSpace(toUser)) return;

        await _messageService.SaveMessageAsync(fromUser, userId, "Приватные", $"{fromUser} -> {toUser}: {message}");
        await Clients.All.SendAsync("ReceiveMessage", "Система", $"# {fromUser} -> {toUser}: {message}");

        _logger.LogInformation($"{fromUser} -> {toUser}: {message}");
    }


    private async Task SendOnlineUsers()
    {
        var users = _onlineUsers.Values.ToList();
        await Clients.All.SendAsync("ReceiveOnlineUsers", users);
    }


    public async Task GetOnlineUsers()
    {
        var users = _onlineUsers.Values.ToList();
        await Clients.Caller.SendAsync("ReceiveOnlineUsers", users);
    }


    public async Task GetHistoryMessagesAsync(string roomName, int count)
    {
        var userName = GetUserName() ?? "Аноним";

        if (string.IsNullOrWhiteSpace(roomName)) roomName = "Общий";

        var history = await _messageService.GetHistoryAsync(roomName, count);
        await Clients.Caller.SendAsync("ReceiveHistory", history);

        _logger.LogInformation($"{userName} запросил историю {roomName} в количестве {count}");
    }

}
