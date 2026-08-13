namespace SearchMessageModel;

public class SearchMessageDto
{
    public string RoomName {get; set;} = string.Empty;
    public string Query {get; set;} = string.Empty;
    public int Limit {get; set;} = 50;
}