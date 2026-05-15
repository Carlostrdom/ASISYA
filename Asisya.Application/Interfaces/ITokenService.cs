using Asisya.Domain.Entities;

namespace Asisya.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    DateTime GetExpiration();
}
