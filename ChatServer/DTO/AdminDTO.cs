namespace AdminUserDTO;

public class AdminUserDto
{
    public int Id {get; set;}
    public string UserName {get; set;} = string.Empty;
    public string Role {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;}
    public int MessageCount {get; set;}
}