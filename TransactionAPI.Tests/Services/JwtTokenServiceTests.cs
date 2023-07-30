using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Application.Services;
using TransactionAPI.Domain.Models;

namespace TransactionAPI.Tests.Services
{
    public class JwtTokenServiceTests
    {
        private JwtTokenService _jwtTokenService;
        private Mock<IConfiguration> _configurationMock;

        [SetUp]
        public void Setup()
        {
            this._configurationMock = new Mock<IConfiguration>();
            this._jwtTokenService = new JwtTokenService(_configurationMock.Object);
        }

        [Test]
        public async Task GenerateToken_ValidUser_ReturnsJwtToken()
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
            var token = await _jwtTokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            Assert.IsNotNull(token);
            Assert.IsNotNull(jwtToken);
            Assert.AreEqual("your_issuer", jwtToken.Issuer);
            Assert.AreEqual("your_audience", jwtToken.Audiences.FirstOrDefault());
            Assert.AreEqual("testuser", jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value);
            Assert.AreEqual("test@example.com", jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value);
        }
    }
}
