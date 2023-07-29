using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Accounts;

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
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, IJwtTokenService jwtTokenService, ILogger<AccountController> logger)
        {
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
        /// <response code="404">User not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginModel)
        {
            try
            {
                var user = await _userService.Authenticate(loginModel);

                if (user != null)
                {
                    var token = await _jwtTokenService.GenerateToken(user);

                    return Ok(token);
                }

                return NotFound("User not found.");
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
                var existingUser = await _userService.GetUserByUsername(registerModel.Username);

                if (existingUser != null)
                {
                    return BadRequest("Username is already taken.");
                }

                var newUser = new User
                {
                    Username = registerModel.Username,
                    Password = registerModel.Password,
                    Email = registerModel.Email,
                };

                var registeredUser = await _userService.Register(newUser);

                if (registeredUser != null)
                {
                    var token = await _jwtTokenService.GenerateToken(registeredUser);
                    return Ok(token);
                }
                else
                {
                    return BadRequest("Registration failed.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in AccountController.Registration(RegisterViewModel registerModel): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");
                _logger.LogTrace(exception.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
