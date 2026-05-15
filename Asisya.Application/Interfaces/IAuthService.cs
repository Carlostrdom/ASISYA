using Asisya.Application.DTOs.Auth;

namespace Asisya.Application.Interfaces;

public interface IAuthService
{
    Task<TokenDto?> LoginAsync(LoginDto dto);
    Task<bool> RegisterAsync(RegisterDto dto);
}
