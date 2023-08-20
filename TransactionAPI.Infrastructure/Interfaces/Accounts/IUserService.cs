using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.ViewModels.Accounts;

namespace TransactionAPI.Infrastructure.Interfaces.Accounts
{
    public interface IUserService
    {
        Task AddUserToDatabase(User newUser);
        Task<User> GetUserByUsername(string username);
        Task<User> GetUserByRefreshToken(string refreshToken);
        Task UpdateRefreshToken(User user);
    }
}
