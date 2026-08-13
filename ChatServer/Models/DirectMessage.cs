namespace DirectMessageModel;

using UserModel;

public class DirectMessage
{
    public int Id {get; set;}
    public int SenderId {get; set;}
    public int ReceiverId {get; set;}
    public string Content {get; set;} = string.Empty;
    public DateTime SentAt {get; set;} = DateTime.UtcNow;
    public bool IsRead {get; set;}
    public DateTime? ReadAt {get; set;}
    public bool IsDeleted {get; set;}

    public User Sender {get; set;} = null!;
    public User Receiver {get; set;} = null!;
}