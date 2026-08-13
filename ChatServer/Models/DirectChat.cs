using UserModel;

namespace DirectChatRoomModel;
public class DicrectChatRoom
{
    public int Id {get; set;}
    public int UserIdOne {get; set;}
    public int UserIdTwo {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime? LastMessageAt {get; set;}

    public User UserOne {get; set;} = null!;
    public User UserTwo {get; set;} = null!;
}