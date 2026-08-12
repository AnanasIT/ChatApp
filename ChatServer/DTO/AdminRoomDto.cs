namespace AdmnRoomDTO;

public class AdminRoomDto
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;}
    public int MessageCount {get; set;}
}