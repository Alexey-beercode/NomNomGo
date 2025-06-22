// TokenServiceTests.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Moq;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Infrastructure.Services;

namespace NomNomGo.IdentityService.Tests;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly TokenService _tokenService;
    private readonly string _secretKey = "ThisIsATestSecretKeyForUnitTestingThatShouldBe32CharsLong";
    private readonly string _issuer = "TestIssuer";
    private readonly string _audience = "TestAudience";
    private readonly string _expirationInMinutes = "60";

    public TokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockCacheService = new Mock<ICacheService>();

        _mockConfiguration.Setup(c => c["Jwt:SecretKey"]).Returns(_secretKey);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns(_issuer);
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(_audience);
        _mockConfiguration.Setup(c => c["Jwt:ExpirationInMinutes"]).Returns(_expirationInMinutes);
        _mockConfiguration.Setup(c => c["Jwt:ServiceTokenExpirationInMinutes"]).Returns(_expirationInMinutes);

        _tokenService = new TokenService(_mockConfiguration.Object, _mockCacheService.Object);
    }
    

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsTrue()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };
        var token = _tokenService.GenerateAccessToken(user, new List<string> { "User" }, new List<string> { "Read" });
        var isValid = _tokenService.ValidateToken(token);
        Assert.True(isValid);
    }
}
