using Microsoft.AspNetCore.Identity.Data;

using ServiceResultModel;
using AuthResponseDTO;
using RegisterDTO;
using LoginDTO;

namespace IAuthServiceModel;


public interface IAuthService
{
    Task<ServiceResult<AuthResponse>>? RegisterAsync(RegisterRequestUser request);
    Task<ServiceResult<AuthResponse>>? LoginAsync(LoginRequestUser request);
}