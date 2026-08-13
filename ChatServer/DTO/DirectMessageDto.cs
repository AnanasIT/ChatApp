namespace DirectMessageDTO;

public class DirectMessageDto
{
    public int Id {get; set;}
    public int SenderId {get; set;}
    public string SenderName {get; set;} = string.Empty;
    public int ReceiverId {get; set;}
    public string ReceiverName {get; set;} = string.Empty;
    public string Content {get; set;} = string.Empty;
    public DateTime SentAt {get; set;}
    public bool IsRead {get; set;}
}