using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Application.Services;
using TransactionAPI.Controllers;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Accounts;

namespace TransactionAPI.Tests.Controllers
{
    public class AccountControllerTests
    {
        private AccountController _accountController;
        private Mock<IUserService> _userServiceMock;
        private Mock<IJwtTokenService> _jwtTokenServiceMock;
        private Mock<ILogger<AccountController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            this._userServiceMock = new Mock<IUserService>();
            this._jwtTokenServiceMock = new Mock<IJwtTokenService>();
            this._loggerMock = new Mock<ILogger<AccountController>>();

            this._accountController = new AccountController(_userServiceMock.Object, _jwtTokenServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Login_ValidUser_ReturnsOkWithToken()
        {
            // Arrange
            var loginModel = new LoginViewModel { Username = "testuser", Password = "testpassword" };
            var user = new User { Username = loginModel.Username, Password = loginModel.Password };
            var token = "generated_jwt_token";

            _userServiceMock.Setup(service => service.Authenticate(loginModel)).ReturnsAsync(user);
            _jwtTokenServiceMock.Setup(service => service.GenerateToken(user)).ReturnsAsync(token);

            // Act
            var actualResult = await _accountController.Login(loginModel);
            var okResult = actualResult as ObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.AreEqual(token, okResult.Value);
        }

        [Test]
        public async Task Login_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var loginModel = new LoginViewModel { Username = "testuser", Password = "testpassword" };
            var exception = new Exception("Some error message");

            _userServiceMock.Setup(service => service.Authenticate(loginModel)).ThrowsAsync(exception);

            // Act
            var actualResult = await _accountController.Login(loginModel);
            var internalServerResult = actualResult as ObjectResult;

            // Assert
            Assert.NotNull(internalServerResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerResult.StatusCode);
            Assert.AreEqual(internalServerResult.Value, exception.Message);
        }

        [Test]
        public async Task Login_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var loginModel = new LoginViewModel { Username = "testuser", Password = "testpassword" };
            User nullUser = null;

            _userServiceMock.Setup(service => service.Authenticate(loginModel)).ReturnsAsync(nullUser);

            // Act
            var actualResult = await _accountController.Login(loginModel);
            var notFoundResult = actualResult as NotFoundObjectResult;

            // Assert
            Assert.NotNull(notFoundResult);
            Assert.AreEqual((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);
            Assert.AreEqual(notFoundResult.Value, "User not found.");
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
            var token = "generated_jwt_token";

            _userServiceMock.Setup(service => service.GetUserByUsername(registerModel.Username)).ReturnsAsync((User)null);

            var registeredUser = new User
            {
                Username = registerModel.Username,
                Password = registerModel.Password,
                Email = registerModel.Email
            };

            _userServiceMock.Setup(service => service.Register(It.IsAny<User>())).ReturnsAsync(registeredUser);
            _jwtTokenServiceMock.Setup(service => service.GenerateToken(registeredUser)).ReturnsAsync(token);

            // Act
            var actualResult = await _accountController.Registration(registerModel);
            var okResult = actualResult as ObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.AreEqual(token, okResult.Value);
        }

        [Test]
        public async Task Registration_ExistingUser_ReturnsBadRequest()
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

            // Act
            var actualResult = await _accountController.Registration(registerModel);
            var badRequestResult = actualResult as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(badRequestResult.Value, "Username is already taken.");
        }

        [Test]
        public async Task Registration_RegistrationFailed_ReturnsBadRequest()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Username = "newuser",
                Password = "password123",
                Email = "test@example.com"
            };

            _userServiceMock.Setup(service => service.GetUserByUsername(registerModel.Username)).ReturnsAsync((User)null);
            _userServiceMock.Setup(service => service.Register(It.IsAny<User>())).ReturnsAsync((User)null);

            // Act
            var actualResult = await _accountController.Registration(registerModel);
            var badRequestResult = actualResult as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(badRequestResult.Value, "Registration failed.");
        }

        [Test]
        public async Task Registration_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Username = "newuser",
                Password = "password123",
                Email = "test@example.com"
            };
            var exception = new Exception("Some error message");

            _userServiceMock.Setup(service => service.GetUserByUsername(registerModel.Username)).ThrowsAsync(exception);

            // Act
            var actualResult = await _accountController.Registration(registerModel);
            var internalServerResult = actualResult as ObjectResult;

            // Assert
            Assert.NotNull(internalServerResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerResult.StatusCode);
            Assert.AreEqual(internalServerResult.Value, exception.Message);
        }
    }
}
