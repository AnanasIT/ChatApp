namespace MessageDTO;

public class MessageDto
{
    public int Id {get; set;}
    public string UserName {get; set;} = string.Empty;
    public string Content {get; set;} = string.Empty;
    public DateTime SentAt {get; set;} = DateTime.UtcNow;

    public bool IsEdited {get; set;} = false;
    public bool IsDeleted {get; set;} = false;
    public DateTime? EditedAt {get; set;}
    public DateTime DeletedAt {get; set;}
}