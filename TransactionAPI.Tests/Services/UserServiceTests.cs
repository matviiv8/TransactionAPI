using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Application.Services;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Context;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Accounts;

namespace TransactionAPI.Tests.Services
{
    public class UserServiceTests
    {
        private UserService _userService;
        private IConfiguration _configuration;
        private IPasswordHasher _passwordHasher;
        private TransactionAPIDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            this._configuration = new ConfigurationBuilder()
                 .AddInMemoryCollection(new Dictionary<string, string>
                 {
                    {"ConnectionStrings:DefaultConnection", "Server=(localdb)\\MSSQLLocalDB;Database=TestDatabase;Trusted_Connection=True;"}
                 })
                 .Build();

            this._passwordHasher = new PasswordHasher();

            var options = new DbContextOptionsBuilder<TransactionAPIDbContext>()
                             .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
                             .Options;
            this._dbContext = new TransactionAPIDbContext(options);

            this._userService = new UserService(_configuration, _passwordHasher);
        }

        [TearDown]
        public void TearDown()
        {
            this._dbContext.Database.EnsureDeleted();
        }

        [Test]
        public async Task Authenticate_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "testpassword"
            };

            var hashedPassword = _passwordHasher.HashPassword(loginModel.Password);
            var user = new User
            {
                Username = loginModel.Username,
                Password = hashedPassword,
                Email = "testemail@gmail.com"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.Authenticate(loginModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loginModel.Username, result.Username);
        }

        [Test]
        public async Task Authenticate_InvalidPassword_ThrowsArgumentException()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            var hashedPassword = "hashed_testpassword";
            var user = new User
            {
                Username = loginModel.Username,
                Password = hashedPassword,
                Email = "testemail@gmail.com"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _userService.Authenticate(loginModel));
        }

        [Test]
        public async Task GetUserByUsername_ExistingUser_ReturnsUser()
        {
            // Arrange
            string username = "existinguser";
            var expectedUser = new User
            {
                Username = username,
                Password = "hashed_password",
                Email = "existinguser@example.com"
            };

            await _dbContext.Users.AddAsync(expectedUser);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByUsername(username);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(username, result.Username);
        }

        [Test]
        public async Task GetUserByUsername_NonExistingUser_ReturnsNull()
        {
            // Arrange
            string username = "nonexistinguser";
            var user = new User
            {
                Username = "testuser",
                Password = "hashed_password",
                Email = "existinguser@example.com"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByUsername(username);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task IsValidEmail_ValidEmail_ReturnsTrue()
        {
            // Arrange
            string validEmail = "test@example.com";

            // Act
            var result = await _userService.IsValidEmail(validEmail);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task IsValidEmail_InvalidEmail_ReturnsFalse()
        {
            // Arrange
            string invalidEmail = "invalid_email";

            // Act
            var result = await _userService.IsValidEmail(invalidEmail);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task Register_ValidUser_ReturnsRegisteredUser()
        {
            // Arrange
            var newUser = new User
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123"
            };

            var hashedPassword = _passwordHasher.HashPassword(newUser.Password);
            var addedUser = new User
            {
                Username = newUser.Username,
                Email = newUser.Email,
                Password = hashedPassword
            };

            // Act
            var result = await _userService.Register(newUser);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(newUser.Username, result.Username);
            Assert.AreEqual(newUser.Email, result.Email);
            Assert.IsNotNull(result.Password);

            var userFromDb = _dbContext.Users.FirstOrDefault(user => user.Username == newUser.Username);
            Assert.IsNotNull(userFromDb);
            Assert.AreEqual(newUser.Username, userFromDb.Username);
            Assert.AreEqual(newUser.Email, userFromDb.Email);
            Assert.AreEqual(result.Password, userFromDb.Password);
        }

        [Test]
        public void Register_ExceptionWhileAddingUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var newUser = new User
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123"
            };

            var exceptionMessage = "Simulated exception while adding user to the database.";

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(service => service.Register(newUser)).ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act & Assert
            var userService = userServiceMock.Object;
            InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await userService.Register(newUser));

            Assert.AreEqual(exceptionMessage, exception.Message);

            var userFromDb = _dbContext.Users.FirstOrDefault(user => user.Username == newUser.Username);
            Assert.IsNull(userFromDb);
        }
    }
}