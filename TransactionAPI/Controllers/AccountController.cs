using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Accounts;
using TransactionAPI.Infrastructure.Interfaces.Authentication;
using TransactionAPI.Infrastructure.Interfaces.Registration;
using TransactionAPI.Infrastructure.ViewModels.Accounts;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Controllers
{
    /// <summary>
    /// Controller for account registration or login.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IRegistrationService _registrationService;
        private readonly IEmailValidationService _emailValidationService;
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthenticationService authenticationService, IRegistrationService registrationService,  ILogger<AccountController> logger,
            IEmailValidationService emailValidationService, IUserService userService, IJwtTokenService jwtTokenService)
        {
            this._authenticationService = authenticationService;
            this._registrationService = registrationService;
            this._emailValidationService = emailValidationService;
            this._userService = userService;
            this._jwtTokenService = jwtTokenService;
            this._logger = logger;
        }

        /// <summary>
        /// Perform user login to the system.
        /// </summary>
        /// <param name="loginModel">Model for user login.</param>
        /// <returns>JWT token for authentication or error message.</returns>
        /// <response code="200">Successful login, returns a JWT token.</response>
        /// <response code="401">Invalid credentials.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginModel)
        {
            try
            {
                var userTokens = await _authenticationService.Authenticate(loginModel);

                if (userTokens == null)
                {
                    return Unauthorized("Invalid credentials");
                }

                return Ok(userTokens);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in AccountController.Login(LoginViewModel loginModel): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");
                _logger.LogTrace(exception.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <param name="registerModel">Model for user registration.</param>
        /// <returns>JWT token for authentication or error message.</returns>
        /// <response code="200">Registration successful, returns a JWT token.</response>
        /// <response code="400">A user with this username already exists or incorrect data was passed.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("registration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Registration([FromBody] RegisterViewModel registerModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var isValidEmail = _emailValidationService.IsValidEmail(registerModel.Email);

                if (!isValidEmail)
                {
                    return BadRequest("Incorrect email format.");
                }

                var existingUser = await _userService.GetUserByUsername(registerModel.Username);

                if (existingUser != null)
                {
                    return BadRequest("Username is already taken.");
                }

                var registeredUserTokens = await _registrationService.Register(registerModel);

                if (registeredUserTokens == null)
                {
                    return BadRequest("Registration failed.");
                }

                return Ok(registeredUserTokens);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in AccountController.Registration(RegisterViewModel registerModel): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");
                _logger.LogTrace(exception.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        /// <summary>
        /// Refresh JWT tokens using a valid refresh token.
        /// </summary>
        /// <param name="refreshTokenModel">Model containing the refresh token.</param>
        /// <returns>New JWT tokens for authentication or an error message.</returns>
        /// <response code="200">Tokens successfully refreshed.</response>
        /// <response code="400">Invalid or missing refresh token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenViewModel refreshTokenModel)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshTokenModel.RefreshToken))
                {
                    return BadRequest("Refresh token is required.");
                }

                var newTokens = await _jwtTokenService.RefreshTokens(refreshTokenModel.RefreshToken);

                if (newTokens != null)
                {
                    return Ok(newTokens);
                }

                return BadRequest("Invalid refresh token.");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in AccountController.RefreshToken(RefreshTokenViewModel refreshTokenModel): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");
                _logger.LogTrace(exception.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
