using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Models;

namespace TransactionAPI.Infrastructure.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> GenerateToken(User user);
    }
}
