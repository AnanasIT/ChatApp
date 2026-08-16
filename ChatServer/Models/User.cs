namespace UserModel;
public class User
{
    public int Id {get; set;}
    public string UserName {get; set;} = string.Empty;
    public string PasswordHash {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Role {get; set;} = "User";
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    public string? Bio {get; set;} = string.Empty;
    public string? AvatarPath {get; set;} = string.Empty;
}
