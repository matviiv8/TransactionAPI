using TransactionAPI.Infrastructure.Interfaces.Accounts;
using TransactionAPI.Infrastructure.Interfaces.Authentication;
using TransactionAPI.Infrastructure.Interfaces.Files;
using TransactionAPI.Infrastructure.Interfaces.Registration;
using TransactionAPI.Infrastructure.Interfaces.Transactions;
using TransactionAPI.Application.Services.Accounts;
using TransactionAPI.Application.Services.Authentication;
using TransactionAPI.Application.Services.Files;
using TransactionAPI.Application.Services.Registration;
using TransactionAPI.Application.Services.Transactions;

namespace TransactionAPI.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITransactionParsingService, TransactionParsingService>();
            services.AddScoped<IEmailValidationService, EmailValidationService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
        }
    }
}
