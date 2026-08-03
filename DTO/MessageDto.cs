namespace MessageDTO;

public class MessageDto
{
    public int Id {get; set;}
    public string UserName {get; set;} = string.Empty;
    public string Content {get; set;} = string.Empty;
    public DateTime SentAt {get; set;} = DateTime.UtcNow;
}