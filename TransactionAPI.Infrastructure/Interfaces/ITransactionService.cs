using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;

namespace TransactionAPI.Infrastructure.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> GetTransactionById(int id);
        Task<Transaction> UpdateTransactionStatus(Transaction transaction, Status? newStatus);
        Task AddTransactionToDatabase(Transaction transaction);
        Task<List<Transaction>> GetTransactionsByFilter(List<Domain.Enums.Type>? types, Status? status, string? clientName = null);
    }
}
