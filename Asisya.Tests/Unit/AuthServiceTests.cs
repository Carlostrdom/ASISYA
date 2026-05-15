using Asisya.Application.DTOs.Auth;
using Asisya.Application.Interfaces;
using Asisya.Application.Services;
using Asisya.Domain.Entities;
using Asisya.Domain.Interfaces;
using Moq;

namespace Asisya.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private AuthService CreateService() =>
        new(_userRepoMock.Object, _hasherMock.Object, _tokenServiceMock.Object);

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("ghost")).ReturnsAsync((User?)null);

        var result = await CreateService().LoginAsync(new LoginDto { Username = "ghost", Password = "pass" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordInvalid()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("carlos"))
                     .ReturnsAsync(new User { Username = "carlos", PasswordHash = "hash" });
        _hasherMock.Setup(h => h.Verify("wrongpass", "hash")).Returns(false);

        var result = await CreateService().LoginAsync(new LoginDto { Username = "carlos", Password = "wrongpass" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsValid()
    {
        var expiration = DateTime.UtcNow.AddHours(1);
        _userRepoMock.Setup(r => r.GetByUsernameAsync("carlos"))
                     .ReturnsAsync(new User { UserID = 1, Username = "carlos", PasswordHash = "hash", Role = "User" });
        _hasherMock.Setup(h => h.Verify("pass123", "hash")).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("jwt.token.here");
        _tokenServiceMock.Setup(t => t.GetExpiration()).Returns(expiration);

        var result = await CreateService().LoginAsync(new LoginDto { Username = "carlos", Password = "pass123" });

        Assert.NotNull(result);
        Assert.Equal("jwt.token.here", result.Token);
        Assert.Equal(expiration, result.ExpiresAt);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsFalse_WhenUserAlreadyExists()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("carlos"))
                     .ReturnsAsync(new User { Username = "carlos" });

        var result = await CreateService().RegisterAsync(new RegisterDto { Username = "carlos", Password = "pass" });

        Assert.False(result);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WhenUsernameAvailable()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("nuevo")).ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.Hash("pass123")).Returns("hashed_pass");

        var result = await CreateService().RegisterAsync(new RegisterDto { Username = "nuevo", Password = "pass123" });

        Assert.True(result);
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Username == "nuevo" && u.PasswordHash == "hashed_pass")), Times.Once);
    }
}
