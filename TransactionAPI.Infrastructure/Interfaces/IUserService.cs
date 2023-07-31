using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.ViewModels.Accounts;

namespace TransactionAPI.Infrastructure.Interfaces
{
    public interface IUserService
    {
        Task<User> Authenticate(LoginViewModel loginModel);
        Task<User> GetUserByUsername(string username);
        Task<User> Register(User newUser);
        Task<bool> IsValidEmail(string email);
    }
}
