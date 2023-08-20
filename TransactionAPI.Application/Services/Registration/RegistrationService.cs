using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Accounts;
using TransactionAPI.Infrastructure.Interfaces.Authentication;
using TransactionAPI.Infrastructure.Interfaces.Registration;
using TransactionAPI.Infrastructure.ViewModels.Accounts;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Application.Services.Registration
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(IUserService userService, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService, ILogger<RegistrationService> logger)
        {
            this._userService = userService;
            this._passwordHasher = passwordHasher;
            this._jwtTokenService = jwtTokenService;
            this._logger = logger;
        }

        public async Task<TokensViewModel> Register(RegisterViewModel registerModel)
        {
            var newUser = new User
            {
                Password = _passwordHasher.HashPassword(registerModel.Password),
                Email = registerModel.Email,
                Username = registerModel.Username,
            };

            var tokens = await _jwtTokenService.GenerateJWTTokens(newUser);
            newUser.RefreshToken = tokens.RefreshToken;

            try
            {
                await _userService.AddUserToDatabase(newUser);

                return tokens;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in RegistrationService.Register(registerModel): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new InvalidOperationException("Failed to add user.", exception);
            }
        }
    }
}
