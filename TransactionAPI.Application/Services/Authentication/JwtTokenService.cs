using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Accounts;
using TransactionAPI.Infrastructure.Interfaces.Authentication;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Application.Services.Authentication
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IConfiguration configuration, IUserService userService, ILogger<JwtTokenService> logger)
        {
            this._configuration = configuration;
            this._userService = userService;
            this._logger = logger;
        }

        public async Task<TokensViewModel> GenerateJWTTokens(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            return new TokensViewModel { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<TokensViewModel> RefreshTokens(string refreshToken)
        {
            var user = await _userService.GetUserByRefreshToken(refreshToken);

            if (user == null)
            {
                return null;
            }

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;

            try
            {
                await _userService.UpdateRefreshToken(user);

                return new TokensViewModel
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in JwtTokenService.RefreshTokens(refreshToken): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new InvalidOperationException("Failed to refresh tokens.", exception);
            }
        }

        public string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var accessToken = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
            );

            return tokenHandler.WriteToken(accessToken);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);

                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
