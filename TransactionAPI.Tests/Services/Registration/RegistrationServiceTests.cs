using Microsoft.Extensions.Logging;
using Moq;
using TransactionAPI.Application.Services.Registration;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Accounts;
using TransactionAPI.Infrastructure.Interfaces.Authentication;
using TransactionAPI.Infrastructure.ViewModels.Accounts;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Tests.Services.Registration
{
    public class RegistrationServiceTests
    {
        private Mock<IUserService> _userServiceMock;
        private Mock<IPasswordHasher> _passwordHasherMock;
        private Mock<IJwtTokenService> _jwtTokenServiceMock;
        private Mock<ILogger<RegistrationService>> _loggerMock;
        private RegistrationService _registrationService;

        [SetUp]
        public void Setup()
        {
            this._userServiceMock = new Mock<IUserService>();
            this._passwordHasherMock = new Mock<IPasswordHasher>();
            this._jwtTokenServiceMock = new Mock<IJwtTokenService>();
            this._loggerMock = new Mock<ILogger<RegistrationService>>();

            this._registrationService = new RegistrationService(
                _userServiceMock.Object,
                _passwordHasherMock.Object,
                _jwtTokenServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Test]
        public async Task Register_ValidModel_ReturnsTokens()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Password = "password",
                Email = "test@example.com",
                Username = "testuser"
            };

            _passwordHasherMock.Setup(service => service.HashPassword(It.IsAny<string>())).Returns("hashed_password");
            _jwtTokenServiceMock.Setup(service => service.GenerateJWTTokens(It.IsAny<User>()))
                .ReturnsAsync(new TokensViewModel { AccessToken = "access_token", RefreshToken = "refresh_token" });
            _userServiceMock.Setup(service => service.AddUserToDatabase(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _registrationService.Register(registerModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("access_token", result.AccessToken);
            Assert.AreEqual("refresh_token", result.RefreshToken);
        }

        [Test]
        public async Task Register_FailedDatabaseAdd_ThrowsException()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Password = "password",
                Email = "test@example.com",
                Username = "testuser"
            };

            _passwordHasherMock.Setup(service => service.HashPassword(It.IsAny<string>())).Returns("hashed_password");
            _jwtTokenServiceMock.Setup(service => service.GenerateJWTTokens(It.IsAny<User>()))
                .ReturnsAsync(new TokensViewModel { AccessToken = "access_token", RefreshToken = "refresh_token" });
            _userServiceMock.Setup(service => service.AddUserToDatabase(It.IsAny<User>())).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _registrationService.Register(registerModel));
        }
    }
}
