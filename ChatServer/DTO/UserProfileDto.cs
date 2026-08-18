namespace UserProfileDTO;

public class UserProfileDto
{
    public int Id {get; set;}
    public string UserName {get; set;} = string.Empty;
    public string Bio {get; set;} = string.Empty;
    public string AvatarURL {get; set;} = string.Empty;
    public string Role {get; set;} = string.Empty;
}

public class UpdateProfileDto
{
    public string Bio {get; set;} = string.Empty;
    public string UserName {get; set;} = string.Empty;
}