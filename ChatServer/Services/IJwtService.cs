namespace IJwtServiceModel;
using UserModel;

public interface IJwtService
{
    string GenerateToken(User User);
}