using TransactionAPI.Application.Services.Accounts;

namespace TransactionAPI.Tests.Services.Accounts
{
    public class EmailValidationServiceTests
    {
        private EmailValidationService _emailValidationService;

        [SetUp]
        public void Setup()
        {
            this._emailValidationService = new EmailValidationService();
        }

        [Test]
        public void IsValidEmail_ValidEmail_ReturnsTrue()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var isValid = _emailValidationService.IsValidEmail(email);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidEmail_InvalidEmail_ReturnsFalse()
        {
            // Arrange
            var email = "invalidemail";

            // Act
            var isValid = _emailValidationService.IsValidEmail(email);

            // Assert
            Assert.IsFalse(isValid);
        }
    }
}
