using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Infrastructure.Interfaces.Accounts;
using TransactionAPI.Infrastructure.Interfaces.Authentication;
using TransactionAPI.Infrastructure.ViewModels.Accounts;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Application.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(IUserService userService, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService, ILogger<AuthenticationService> logger)
        {
            this._userService = userService;
            this._passwordHasher = passwordHasher;
            this._jwtTokenService = jwtTokenService;
            this._logger = logger;
        }

        public async Task<TokensViewModel> Authenticate(LoginViewModel loginModel)
        {
            try
            {
                var user = await _userService.GetUserByUsername(loginModel.Username);

                if (user != null && _passwordHasher.VerifyPassword(loginModel.Password, user.Password))
                {
                    var userTokens = await _jwtTokenService.GenerateJWTTokens(user);

                    return userTokens;
                }

                return null;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in AuthenticationService.Authenticate(loginModel): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error during authentication.", exception);
            }
        }
    }
}
