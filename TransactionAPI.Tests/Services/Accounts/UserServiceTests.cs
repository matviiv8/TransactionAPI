using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionAPI.Application.Services.Accounts;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Context;

namespace TransactionAPI.Tests.Services.Accounts
{
    public class UserServiceTests
    {
        private UserService _userService;
        private IConfiguration _configuration;
        private TransactionAPIDbContext _dbContext;
        private Mock<ILogger<UserService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            this._configuration = new ConfigurationBuilder()
                 .AddInMemoryCollection(new Dictionary<string, string>
                 {
                    {"ConnectionStrings:DefaultConnection", "Server=(localdb)\\MSSQLLocalDB;Database=TestDatabase;Trusted_Connection=True;"}
                 })
                 .Build();

            var options = new DbContextOptionsBuilder<TransactionAPIDbContext>()
                             .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
                             .Options;
            this._dbContext = new TransactionAPIDbContext(options);

            this._loggerMock = new Mock<ILogger<UserService>>();
            this._userService = new UserService(_configuration, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this._dbContext.Database.EnsureDeleted();
        }

        [Test]
        public async Task GetUserByRefreshToken_ExistingUser_ReturnsUser()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var expectedUser = new User
            {
                Username = "existinguser",
                Password = "hashed_password",
                Email = "existinguser@example.com",
                RefreshToken = refreshToken
            };

            await _dbContext.Users.AddAsync(expectedUser);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByRefreshToken(refreshToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser.Username, result.Username);
            Assert.AreEqual(expectedUser.Email, result.Email);
            Assert.AreEqual(expectedUser.RefreshToken, result.RefreshToken);
        }

        [Test]
        public async Task GetUserByRefreshToken_NonExistingUser_ReturnsNull()
        {
            // Arrange
            var refreshToken = "non_existing_refresh_token";
            var user = new User
            {
                Username = "testuser",
                Password = "hashed_password",
                Email = "existinguser@example.com",
                RefreshToken = "valid_refresh_token"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByRefreshToken(refreshToken);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetUserByRefreshToken_DatabaseError_ThrowsApplicationExceptionAndReturnsNull()
        {
            // Arrange
            _dbContext.Database.EnsureDeleted();

            // Act
            var exception = Assert.ThrowsAsync<ApplicationException>(async () => await _userService.GetUserByRefreshToken(It.IsAny<string>()));

            // Assert
            Assert.AreEqual("Error while fetching user by refresh token from the database.", exception.Message);
        }

        [Test]
        public async Task GetUserByUsername_ExistingUser_ReturnsUser()
        {
            // Arrange
            string username = "existinguser";
            string password = "password";
            var expectedUser = new User
            {
                Username = username,
                Password = password,
                Email = "existinguser@example.com",
                RefreshToken = "valid_refresh_token"
            };

            await _dbContext.Users.AddAsync(expectedUser);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByUsername(username);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(username, result.Username);
            Assert.AreEqual(expectedUser.Email, result.Email);
            Assert.AreEqual(expectedUser.RefreshToken, result.RefreshToken);
            Assert.AreEqual(expectedUser.Password, result.Password);
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
                Email = "existinguser@example.com",
                RefreshToken = "valid_refresh_token"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByUsername(username);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetUserByUsername_DatabaseError_ThrowsApplicationExceptionAndReturnsNull()
        {
            // Arrange
            _dbContext.Database.EnsureDeleted();

            // Act
            var exception = Assert.ThrowsAsync<ApplicationException>(async () => await _userService.GetUserByUsername(It.IsAny<string>()));

            // Assert
            Assert.AreEqual("Error while adding user to the database.", exception.Message);
        }

        [Test]
        public async Task AddUserToDatabase_ValidUser_ReturnsRegisteredUser()
        {
            // Arrange
            string password = "password";
            var newUser = new User
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = password,
                RefreshToken = "valid_refresh_token"
            };

            // Act
            await _userService.AddUserToDatabase(newUser);

            // Assert
            var userFromDb = await _dbContext.Users.FirstOrDefaultAsync(user => user.Username == newUser.Username);
            Assert.IsNotNull(userFromDb);
            Assert.AreEqual(newUser.Username, userFromDb.Username);
            Assert.AreEqual(newUser.Email, userFromDb.Email);
            Assert.AreEqual(newUser.RefreshToken, userFromDb.RefreshToken);
            Assert.AreEqual(newUser.Password, userFromDb.Password);
        }

        [Test]
        public async Task AddUserToDatabase_DatabaseError_ThrowsApplicationException()
        {
            // Arrange
            _dbContext.Database.EnsureDeleted();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(async () => await _userService.AddUserToDatabase(It.IsAny<User>()));
            Assert.AreEqual("Error while adding user to the database.", exception.Message);
        }

        [Test]
        public async Task UpdateRefreshToken_ValidUser_UpdatesRefreshToken()
        {
            // Arrange
            string username = "existinguser";
            string password = "password";
            var expectedUser = new User
            {
                Username = username,
                Password = password,
                Email = "existinguser@example.com",
                RefreshToken = "valid_refresh_token"
            };

            await _dbContext.Users.AddAsync(expectedUser);
            await _dbContext.SaveChangesAsync();

            expectedUser.RefreshToken = "new_refresh_token";

            // Act
            await _userService.UpdateRefreshToken(expectedUser);

            // Assert
            var updatedUser = await _userService.GetUserByUsername(username);
            Assert.AreEqual(expectedUser.RefreshToken, updatedUser.RefreshToken);
        }

        [Test]
        public async Task UpdateRefreshToken_DatabaseError_ThrowsApplicationException()
        {
            // Arrange
            _dbContext.Database.EnsureDeleted();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(async () => await _userService.UpdateRefreshToken(It.IsAny<User>()));
            Assert.AreEqual("Error while updating user's refresh token in the database.", exception.Message);
        }
    }
}