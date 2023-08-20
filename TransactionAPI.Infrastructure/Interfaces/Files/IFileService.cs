using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Models;

namespace TransactionAPI.Infrastructure.Interfaces.Files
{
    public interface IFileService
    {
        Task ProcessExcelFile(Stream fileStream);
        Task ExportTransactionsToCsv(List<Transaction> transactions, string filePath);
    }
}
