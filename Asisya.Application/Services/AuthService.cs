using Asisya.Application.DTOs.Auth;
using Asisya.Application.Interfaces;
using Asisya.Domain.Entities;
using Asisya.Domain.Interfaces;

namespace Asisya.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<TokenDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username);
        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            return null;

        return new TokenDto
        {
            Token = _tokenService.GenerateToken(user),
            ExpiresAt = _tokenService.GetExpiration()
        };
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existing is not null) return false;

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        return true;
    }
}
