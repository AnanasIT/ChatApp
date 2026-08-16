namespace DirectChatRoomDTO;

public class DirectChatRoomDto
{
    public int Id {get; set;}
    public int OtherUserId {get; set;}
    public string OtherUserName {get; set;} = string.Empty;
    public string LastMessage {get; set;} = string.Empty;
    public DateTime LastMessageAt  {get; set;}
    public int UnreadCount {get; set;}
}