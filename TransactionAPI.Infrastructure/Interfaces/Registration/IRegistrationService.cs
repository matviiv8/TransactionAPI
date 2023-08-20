using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Infrastructure.ViewModels.Accounts;
using TransactionAPI.Infrastructure.ViewModels.Tokens;

namespace TransactionAPI.Infrastructure.Interfaces.Registration
{
    public interface IRegistrationService
    {
        Task<TokensViewModel> Register(RegisterViewModel registerModel);
    }
}
