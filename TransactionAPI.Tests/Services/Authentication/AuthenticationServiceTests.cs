using Microsoft.Extensions.Logging;
using Moq;
using TransactionAPI.Application.Services.Authentication;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Accounts;
using TransactionAPI.Infrastructure.Interfaces.Authentication;
using TransactionAPI.Infrastructure.ViewModels.Accounts;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Tests.Services.Authentication
{
    public class AuthenticationServiceTests
    {
        private Mock<IUserService> _userServiceMock;
        private Mock<IPasswordHasher> _passwordHasherMock;
        private Mock<IJwtTokenService> _jwtTokenServiceMock;
        private Mock<ILogger<AuthenticationService>> _loggerMock;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            this._userServiceMock = new Mock<IUserService>();
            this._passwordHasherMock = new Mock<IPasswordHasher>();
            this._jwtTokenServiceMock = new Mock<IJwtTokenService>();
            this._loggerMock = new Mock<ILogger<AuthenticationService>>();

            this._authenticationService = new AuthenticationService(_userServiceMock.Object, _passwordHasherMock.Object, _jwtTokenServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Authenticate_ValidLogin_ReturnsTokensViewModel()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "password123"
            };

            _userServiceMock.Setup(service => service.GetUserByUsername(loginModel.Username)).ReturnsAsync(new User { Username = loginModel.Username, Password = "hashed_password" });
            _passwordHasherMock.Setup(service => service.VerifyPassword(loginModel.Password, "hashed_password")).Returns(true);
            _jwtTokenServiceMock.Setup(service => service.GenerateJWTTokens(It.IsAny<User>())).ReturnsAsync(new TokensViewModel());

            // Act
            var tokensViewModel = await _authenticationService.Authenticate(loginModel);

            // Assert
            Assert.NotNull(tokensViewModel);
            Assert.IsInstanceOf<TokensViewModel>(tokensViewModel);
        }

        [Test]
        public async Task Authenticate_ExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "password123"
            };

            _userServiceMock.Setup(service => service.GetUserByUsername(loginModel.Username)).ThrowsAsync(new Exception("Some error"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _authenticationService.Authenticate(loginModel));

            Assert.NotNull(exception.InnerException);
            Assert.IsInstanceOf<Exception>(exception.InnerException);
            Assert.AreEqual("Error during authentication.", exception.Message);
        }

        [Test]
        public async Task Authenticate_InvalidLogin_ReturnsNull()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "wrong_password"
            };

            _userServiceMock.Setup(service => service.GetUserByUsername(loginModel.Username)).ReturnsAsync(new User { Username = loginModel.Username, Password = "hashed_password" });
            _passwordHasherMock.Setup(service => service.VerifyPassword(loginModel.Password, "hashed_password")).Returns(false);

            // Act
            var tokensViewModel = await _authenticationService.Authenticate(loginModel);

            // Assert
            Assert.Null(tokensViewModel);
        }
    }
}
