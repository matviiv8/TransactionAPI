using TransactionAPI.Application.Services.Accounts;

namespace TransactionAPI.Tests.Services.Accounts
{
    public class PasswordHasherServiceTests
    {
        private PasswordHasher _passwordHasher;

        [SetUp]
        public void Setup()
        {
            this._passwordHasher = new PasswordHasher();
        }

        [Test]
        public void HashPassword_ValidPassword_ReturnsNonEmptyString()
        {
            // Arrange
            var password = "testpassword";

            // Act
            var hashedPassword = _passwordHasher.HashPassword(password);

            // Assert
            Assert.IsNotNull(hashedPassword);
            Assert.IsNotEmpty(hashedPassword);
        }

        [Test]
        public void HashPassword_SamePasswordTwice_ReturnsSameHash()
        {
            // Arrange
            var password = "testpassword";

            // Act
            var hashedPassword1 = _passwordHasher.HashPassword(password);
            var hashedPassword2 = _passwordHasher.HashPassword(password);

            // Assert
            Assert.IsNotNull(hashedPassword1);
            Assert.IsNotNull(hashedPassword2);
            Assert.AreEqual(hashedPassword1, hashedPassword2);
        }

        [Test]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "testpassword";

            // Act
            var hashedPassword = _passwordHasher.HashPassword(password);
            var result = _passwordHasher.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyPassword_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var correctPassword = "testpassword";
            var incorrectPassword = "wrongpassword";

            // Act
            var hashedPassword = _passwordHasher.HashPassword(correctPassword);
            var result = _passwordHasher.VerifyPassword(incorrectPassword, hashedPassword);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
