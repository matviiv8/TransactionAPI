using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TransactionAPI.Infrastructure.Interfaces.Accounts;

namespace TransactionAPI.Application.Services.Accounts
{
    public class EmailValidationService : IEmailValidationService
    {
        public bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Match match = Regex.Match(email, pattern);

            return match.Success;
        }
    }
}
