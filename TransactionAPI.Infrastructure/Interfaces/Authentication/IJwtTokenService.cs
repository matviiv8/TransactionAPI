using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Infrastructure.Interfaces.Authentication
{
    public interface IJwtTokenService
    {
        Task<TokensViewModel> GenerateJWTTokens(User user);
        Task<TokensViewModel> RefreshTokens(string refreshToken);
    }
}
