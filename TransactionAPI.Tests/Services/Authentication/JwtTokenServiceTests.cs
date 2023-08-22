using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TransactionAPI.Application.Services.Authentication;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Accounts;

namespace TransactionAPI.Tests.Services.Authentication
{
    public class JwtTokenServiceTests
    {
        private JwtTokenService _jwtTokenService;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<ILogger<JwtTokenService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            this._configurationMock = new Mock<IConfiguration>();
            this._userServiceMock = new Mock<IUserService>();
            this._loggerMock = new Mock<ILogger<JwtTokenService>>();

            this._jwtTokenService = new JwtTokenService(_configurationMock.Object, _userServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GenerateJWTTokens_ValidUser_ReturnsTokensViewModel()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com"
            }; 

            _configurationMock.Setup(config => config["Jwt:Key"]).Returns("your_secret_key_with_at_least_128_bits");
            _configurationMock.Setup(config => config["Jwt:Issuer"]).Returns("your_issuer");
            _configurationMock.Setup(config => config["Jwt:Audience"]).Returns("your_audience");

            // Act
            var tokens = await _jwtTokenService.GenerateJWTTokens(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokens.AccessToken);

            // Assert
            Assert.IsNotNull(tokens);
            Assert.IsNotNull(tokens.RefreshToken);
            Assert.IsNotNull(tokens.AccessToken);
            Assert.IsNotNull(jwtToken);
            Assert.AreEqual("your_issuer", jwtToken.Issuer);
            Assert.AreEqual("your_audience", jwtToken.Audiences.FirstOrDefault());
            Assert.AreEqual("testuser", jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value);
            Assert.AreEqual("test@example.com", jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value);
        }

        [Test]
        public async Task RefreshTokens_InvalidRefreshToken_ReturnsNull()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";

            _userServiceMock.Setup(service => service.GetUserByRefreshToken(refreshToken)).ReturnsAsync((User)null);

            // Act
            var tokensViewModel = await _jwtTokenService.RefreshTokens(refreshToken);

            // Assert
            Assert.IsNull(tokensViewModel);
        }

        [Test]
        public async Task RefreshTokens_UpdateRefreshTokenFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var refreshToken = "fake_refresh_token";
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com"
            };

            _configurationMock.Setup(config => config["Jwt:Key"]).Returns("your_secret_key_with_at_least_128_bits");
            _configurationMock.Setup(config => config["Jwt:Issuer"]).Returns("your_issuer");
            _configurationMock.Setup(config => config["Jwt:Audience"]).Returns("your_audience");
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            _userServiceMock.Setup(service => service.GetUserByRefreshToken(refreshToken)).ReturnsAsync(user);
            _userServiceMock.Setup(service => service.UpdateRefreshToken(user)).ThrowsAsync(new InvalidOperationException("Error while updating refresh token"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _jwtTokenService.RefreshTokens(refreshToken));
            Assert.AreEqual("Failed to refresh tokens.", exception.Message);
        }

        [Test]
        public async Task RefreshTokens_ValidRefreshToken_ReturnsTokensViewModel()
        {
            // Arrange
            var refreshToken = "fake_refresh_token";
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com"
            };

            _configurationMock.Setup(config => config["Jwt:Key"]).Returns("your_secret_key_with_at_least_128_bits");
            _configurationMock.Setup(config => config["Jwt:Issuer"]).Returns("your_issuer");
            _configurationMock.Setup(config => config["Jwt:Audience"]).Returns("your_audience");
            _userServiceMock.Setup(service => service.GetUserByRefreshToken(refreshToken)).ReturnsAsync(user);

            // Act
            var tokensViewModel = await _jwtTokenService.RefreshTokens(refreshToken);

            // Assert
            Assert.IsNotNull(tokensViewModel);
            Assert.AreEqual(user.RefreshToken, tokensViewModel.RefreshToken);
        }
    }
}
