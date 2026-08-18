namespace MessageModel;

using UserModel;
using RoomModel;

public class Message
{
    public int Id {get; set;}
    public string Content {get; set;} = string.Empty;
    public DateTime SentAt {get; set;} = DateTime.UtcNow;

    public bool IsEdited {get; set;} = false;
    public bool IsDeleted {get; set;} = false;
    public DateTime? EditedAt {get; set;}
    public DateTime DeletedAt {get; set;}

    public string? ImageUrl {get; set;}
    public string? ThumbnaiUrl {get; set;}

    public int UserId {get; set;}
    public User User {get; set;} = null!;

    public int RoomId {get; set;}
    public Room Room {get; set;} = null!;
}