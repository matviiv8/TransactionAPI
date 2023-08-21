using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using TransactionAPI.Controllers;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Accounts;
using TransactionAPI.Infrastructure.Interfaces.Authentication;
using TransactionAPI.Infrastructure.Interfaces.Registration;
using TransactionAPI.Infrastructure.ViewModels.Accounts;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Tests.Controllers
{
    public class AccountControllerTests
    {
        private AccountController _accountController;
        private Mock<IAuthenticationService> _authenticationServiceMock;
        private Mock<IRegistrationService> _registrationServiceMock;
        private Mock<IEmailValidationService> _emailValidationServiceMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IJwtTokenService> _jwtTokenServiceMock;
        private Mock<ILogger<AccountController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _registrationServiceMock = new Mock<IRegistrationService>();
            _emailValidationServiceMock = new Mock<IEmailValidationService>();
            _userServiceMock = new Mock<IUserService>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _loggerMock = new Mock<ILogger<AccountController>>();

            _accountController = new AccountController(
                _authenticationServiceMock.Object,
                _registrationServiceMock.Object,
                _loggerMock.Object,
                _emailValidationServiceMock.Object,
                _userServiceMock.Object,
                _jwtTokenServiceMock.Object
            );
        }

        [Test]
        public async Task Login_ValidUser_ReturnsOkWithToken()
        {
            // Arrange
            var loginModel = new LoginViewModel { Username = "testuser", Password = "testpassword" };
            var userTokens = new TokensViewModel { AccessToken = "access_token", RefreshToken = "refresh_token" };

            _authenticationServiceMock.Setup(service => service.Authenticate(loginModel)).ReturnsAsync(userTokens);

            // Act
            var result = await _accountController.Login(loginModel);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.AreEqual(userTokens, okResult.Value);
        }

        [Test]
        public async Task Login_UserWithAllEmptyFields_ReturnsUnauthorizedWithInvalidCredentialsMessage()
        {
            // Arrange
            var loginModel = new LoginViewModel { Username = string.Empty, Password = string.Empty };

            // Act
            var result = await _accountController.Login(loginModel);
            var unauthorizedResult = result as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(unauthorizedResult);
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, unauthorizedResult.StatusCode);
            Assert.AreEqual("Invalid credentials", unauthorizedResult.Value);
        }

        [Test]
        public async Task Login_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var loginModel = new LoginViewModel { Username = "testuser", Password = "testpassword" };
            var exception = new Exception("Some error message");

            _authenticationServiceMock.Setup(service => service.Authenticate(loginModel)).ThrowsAsync(exception);

            // Act
            var result = await _accountController.Login(loginModel);
            var internalServerErrorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(internalServerErrorResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerErrorResult.StatusCode);
            Assert.AreEqual(exception.Message, internalServerErrorResult.Value);
        }

        [Test]
        public async Task Registration_NewUser_ReturnsOkWithToken()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Username = "newuser",
                Password = "password123",
                Email = "test@example.com"
            };
            var userTokens = new TokensViewModel { AccessToken = "access_token", RefreshToken = "refresh_token" };

            _emailValidationServiceMock.Setup(service => service.IsValidEmail(registerModel.Email)).Returns(true);
            _userServiceMock.Setup(service => service.GetUserByUsername(registerModel.Username)).ReturnsAsync((User)null);
            _registrationServiceMock.Setup(service => service.Register(registerModel)).ReturnsAsync(userTokens);

            // Act
            var result = await _accountController.Registration(registerModel);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.AreEqual(userTokens, okResult.Value);
        }

        [Test]
        public async Task Registration_ExistingUser_ReturnsBadRequestWithUsernameTakenMessage()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Username = "existinguser",
                Password = "password123",
                Email = "test@example.com"
            };

            var existingUser = new User
            {
                Username = registerModel.Username,
                Password = "hashedpassword",
                Email = registerModel.Email
            };

            _userServiceMock.Setup(service => service.GetUserByUsername(registerModel.Username)).ReturnsAsync(existingUser);
            _emailValidationServiceMock.Setup(service => service.IsValidEmail(registerModel.Email)).Returns(true);

            // Act
            var result = await _accountController.Registration(registerModel);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(badRequestResult.Value, "Username is already taken.");
        }

        [Test]
        public async Task Registration_InvalidModel_ReturnsBadRequestWithModelStateErrors()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Username = string.Empty,
                Password = "password123",
                Email = "test@example.com"
            };

            _accountController.ModelState.AddModelError("Username", "Username is required.");
            _accountController.ModelState.AddModelError("Email", "Email is required.");

            // Act
            var result = await _accountController.Registration(registerModel);
            var badRequestResult = result as BadRequestObjectResult;
            var errors = (SerializableError)badRequestResult.Value;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);

            Assert.IsTrue(errors.ContainsKey("Username"));
            Assert.IsTrue(errors.ContainsKey("Email"));

            var usernameError = errors["Username"];
            Assert.AreEqual("Username is required.", ((string[])usernameError)[0]);

            var emailError = errors["Email"];
            Assert.AreEqual("Email is required.", ((string[])emailError)[0]);
        }

        [Test]
        public async Task Registration_RegistrationFailed_ReturnsBadRequestWithRegistrationFailedMessage()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Username = "newuser",
                Password = "password123",
                Email = "test@example.com"
            };

            _userServiceMock.Setup(service => service.GetUserByUsername(registerModel.Username)).ReturnsAsync((User)null);
            _emailValidationServiceMock.Setup(service => service.IsValidEmail(registerModel.Email)).Returns(true);
            _registrationServiceMock.Setup(service => service.Register(It.IsAny<RegisterViewModel>())).ReturnsAsync((TokensViewModel)null);

            // Act
            var result = await _accountController.Registration(registerModel);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(badRequestResult.Value, "Registration failed.");
        }

        [Test]
        public async Task Registration_InvalidEmail_ReturnsBadRequestWithIncorrectEmailFormatMessage()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Username = "newuser",
                Password = "password123",
                Email = "testexamplecom"
            };

            _emailValidationServiceMock.Setup(service => service.IsValidEmail(registerModel.Email)).Returns(false);

            // Act
            var result = await _accountController.Registration(registerModel);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(badRequestResult.Value, "Incorrect email format.");
        }

        [Test]
        public async Task Registration_ExceptionThrown_ReturnsInternalServerErrorWithExceptionMessage()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Username = "newuser",
                Password = "password123",
                Email = "test@example.com"
            };
            var exception = new Exception("Some error message");

            _emailValidationServiceMock.Setup(service => service.IsValidEmail(registerModel.Email)).Returns(true);
            _userServiceMock.Setup(service => service.GetUserByUsername(registerModel.Username)).ThrowsAsync(exception);

            // Act
            var result = await _accountController.Registration(registerModel);
            var internalServerResult = result as ObjectResult;

            // Assert
            Assert.NotNull(internalServerResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerResult.StatusCode);
            Assert.AreEqual(internalServerResult.Value, exception.Message);
        }

        [Test]
        public async Task RefreshToken_ValidRefreshToken_ReturnsNewTokens()
        {
            // Arrange
            var refreshTokenModel = new RefreshTokenViewModel { RefreshToken = "valid_refresh_token" };
            var newTokens = new TokensViewModel { AccessToken = "new_access_token", RefreshToken = "new_refresh_token" };

            _jwtTokenServiceMock.Setup(service => service.RefreshTokens(refreshTokenModel.RefreshToken)).ReturnsAsync(newTokens);

            // Act
            var result = await _accountController.RefreshToken(refreshTokenModel);
            var okResult = result as ObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);

            var returnedTokens = okResult.Value as TokensViewModel;
            Assert.NotNull(returnedTokens);
            Assert.AreEqual(newTokens.AccessToken, returnedTokens.AccessToken);
            Assert.AreEqual(newTokens.RefreshToken, returnedTokens.RefreshToken);
        }

        [Test]
        public async Task RefreshToken_InvalidRefreshToken_ReturnsBadRequestWithInvalidRefreshTokenMessage()
        {
            // Arrange
            var refreshTokenModel = new RefreshTokenViewModel { RefreshToken = string.Empty };

            // Act
            var result = await _accountController.RefreshToken(refreshTokenModel);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual("Refresh token is required.", badRequestResult.Value);
        }

        [Test]
        public async Task RefreshToken_RefreshTokenServiceReturnsNull_ReturnsBadRequestWithInvalidRefreshTokenMessage()
        {
            // Arrange
            var refreshTokenModel = new RefreshTokenViewModel { RefreshToken = "valid_refresh_token" };

            _jwtTokenServiceMock.Setup(service => service.RefreshTokens(refreshTokenModel.RefreshToken)).ReturnsAsync((TokensViewModel)null);

            // Act
            var result = await _accountController.RefreshToken(refreshTokenModel);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid refresh token.", badRequestResult.Value);
        }

        [Test]
        public async Task RefreshToken_ExceptionThrown_ReturnsInternalServerErrorWithExceptionMessage()
        {
            // Arrange
            var refreshTokenModel = new RefreshTokenViewModel { RefreshToken = "valid_refresh_token" };
            var exception = new Exception("Some error message");

            _jwtTokenServiceMock.Setup(service => service.RefreshTokens(refreshTokenModel.RefreshToken)).ThrowsAsync(exception);

            // Act
            var result = await _accountController.RefreshToken(refreshTokenModel);
            var internalServerResult = result as ObjectResult;

            // Assert
            Assert.NotNull(internalServerResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerResult.StatusCode);
            Assert.AreEqual(exception.Message, internalServerResult.Value);
        }

    }
}
