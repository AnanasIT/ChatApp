using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatClient;

class ClientProgram
{
    private static HubConnection? _connection;
    private static string _token = "";
    private static string _userName = "";
    private static string _currentRoom = "";

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "💬 Чат Клиент";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔═══════════════════════════════════════╗");
        Console.WriteLine("║       💬  ДОБРО ПОЖАЛОВАТЬ В ЧАТ      ║");
        Console.WriteLine("╚═══════════════════════════════════════╝");
        Console.ResetColor();

        while (string.IsNullOrEmpty(_token))
        {
            Console.WriteLine("\n[1] Вход");
            Console.WriteLine("\n[2] Регистрация");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await Login();
                    break;
                
                case "2":
                    await Register();
                    break;
                
                default:
                    Console.WriteLine("❌ Неверный выбор!");
                    break;
            }
        }


        _connection = new HubConnectionBuilder()
            .WithUrl($"http://localhost:5000/chat?access_token={_token}")
            .WithAutomaticReconnect()
            .Build();
        
        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
           var time = DateTime.Now.ToString("HH:mm:ss");
           var color = user == _userName ? ConsoleColor.Green : ConsoleColor.White;
           
           Console.ForegroundColor = color;
           Console.WriteLine($"[{time}] [{user}]: {message}");
           Console.ResetColor();
        });

        _connection.On<List<object>>("ReceiveHistory", (history) =>
        {
           Console.ForegroundColor = ConsoleColor.DarkGray;
           Console.WriteLine($"\n📜 ИСТОРИЯ ({history.Count} сообещний):");
           Console.WriteLine(new string('-', 40));
           
           foreach (var message in history)
           {
              var props = message.GetType().GetProperties();
              var username = props.First(p => p.Name == "UserName").GetValue(message)?.ToString();
              var content = props.First(p => p.Name == "Content").GetValue(message)?.ToString();
              Console.WriteLine($"{username} {content}");   
           } 

           Console.WriteLine(new string('-', 40));
           Console.ResetColor();
        });

        _connection.On<List<string>>("ReceiveOnlineUsers", (users) =>
        {
           Console.ForegroundColor = ConsoleColor.Cyan;
           Console.WriteLine($"\n👥 Online ({users.Count}):");
           
           foreach (var user in users) {
                Console.WriteLine($"🟢 {user}"); 
           }

           Console.ResetColor();
        });


    }

    static async Task Register()
    {
        Console.WriteLine("👤 Введите ваше имя: ");
        var username = Console.ReadLine()?.Trim() ?? "";

        Console.WriteLine("📧 Введите ваш Email: ");
        var email = Console.ReadLine()?.Trim() ?? "";

        Console.WriteLine("🔑 Введите пароль: ");
        var password = Console.ReadLine()?.Trim() ?? "";

        try
        {
            using var http = new HttpClient();
            var response = await http.PostAsJsonAsync("http://localhost:5000/register", new
            {
               UserName = username,
               Email = email,
               Password = password 
            });

            switch (response.IsSuccessStatusCode)
            {
                case true:
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    
                    _token = result?.Token ?? "";
                    _userName = username;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Регистрация прошла успешно");
                    Console.ResetColor();
                    break;


                case false:
                    var error = await response.Content.ReadAsStringAsync();
                    
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Ошибка регистрации: {error}");
                    Console.ResetColor();
                    break;
            }
        }

        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Ошибка: {ex.Message}");
            Console.ResetColor();
        }
    }


    static async Task Login()
    {
         Console.WriteLine("👤 Введите ваше имя: ");
         var username = Console.ReadLine()?.Trim() ?? "";

         Console.WriteLine("🔑 Введите пароль: ");
         var password = Console.ReadLine()?.Trim() ?? "";

         try
         {
             using var http = new HttpClient();
             var response = await http.PostAsJsonAsync("http://localhost:5000/login", new
             {
                UserName = username,
                Password = password 
             });

             switch (response.IsSuccessStatusCode)
             {
                case true:
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    _token = result?.Token ?? "";
                    _userName = username;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Вход в аккаунт успешный!");
                    Console.ResetColor();
                    
                    break; 
                

                case false:
                    var error = await response.Content.ReadAsStringAsync();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Ошибка регистрации: {error}");
                    Console.ResetColor();
                    break;
             }
         }

         catch (Exception ex)
         {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Ошибка: {ex.Message}");
            Console.ResetColor();
         }
    }
}




// =============== DTO ДЛЯ ОТВЕТА ================
public class AuthResponse
{
    public string Token {get; set;} = string.Empty;
    public string UserName {get; set;} = string.Empty;
    public string Role {get; set;} = string.Empty;
}