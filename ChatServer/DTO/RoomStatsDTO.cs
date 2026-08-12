namespace RoomStatsDTO;

public class RoomStatsDto
{
    public string RoomName {get; set;} = string.Empty;
    public int TotalMessages {get; set;}
    public int UniqueUsers {get; set;}
    public List<string> Users {get; set;} = new();
}