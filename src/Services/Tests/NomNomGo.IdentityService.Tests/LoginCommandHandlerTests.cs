// LoginCommandHandlerTests.cs
using System.Security.Authentication;
using System.Text;
using AutoMapper;
using Moq;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Login;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Domain.Entities.Relationships;
using NomNomGo.IdentityService.Domain.Exceptions;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;
using AuthenticationException = NomNomGo.IdentityService.Domain.Exceptions.AuthenticationException;

namespace NomNomGo.IdentityService.Tests;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<ITokenService> _mockTokenService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly Mock<IRoleRepository> _mockRoleRepository = new();
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockUnitOfWork.Setup(x => x.UserRepository).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.RoleRepository).Returns(_mockRoleRepository.Object);
        _mockUnitOfWork.Setup(x => x.RefreshTokenRepository).Returns(_mockRefreshTokenRepository.Object);

        _handler = new LoginCommandHandler(
            _mockUnitOfWork.Object,
            _mockTokenService.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsAuthenticationException()
    {
        var user = new User { Username = "testuser", PasswordHash = CreatePasswordHash("password123") };
        _mockUserRepository.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await Assert.ThrowsAsync<AuthenticationException>(() => _handler.Handle(new LoginCommand { Login = "testuser", Password = "wrongpass" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_BlockedUser_ThrowsAuthenticationException()
    {
        var user = new User { Username = "testuser", PasswordHash = CreatePasswordHash("password123"), IsBlocked = true, BlockedUntil = DateTime.UtcNow.AddMinutes(30) };
        _mockUserRepository.Setup(r => r.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => _handler.Handle(new LoginCommand { Login = "testuser", Password = "password123" }, CancellationToken.None));
        Assert.Contains("заблокирована", ex.Message);
    }

    private string CreatePasswordHash(string password)
    {
        var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        var salt = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return $"{hash}.{salt}";
    }
}
