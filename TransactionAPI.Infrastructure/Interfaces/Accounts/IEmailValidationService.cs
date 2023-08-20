using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAPI.Infrastructure.Interfaces.Accounts
{
    public interface IEmailValidationService
    {
        bool IsValidEmail(string email);
    }
}
