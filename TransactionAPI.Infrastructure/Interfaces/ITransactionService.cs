using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Infrastructure.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> GetTransactionById(int id);
        Task MergeTransaction(Transaction transaction);
        Task<Transaction> UpdateTransactionStatus(Transaction transaction, Status? newStatus);
        Task<List<Transaction>> GetTransactionsByFilter(List<Type>? types, Status? status, string? clientName = null);
    }
}
