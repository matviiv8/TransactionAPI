using OfficeOpenXml;
using TransactionAPI.Domain.Models;

namespace TransactionAPI.Infrastructure.Interfaces.Transactions
{
    public interface ITransactionParsingService
    {
        Transaction ParseTransactionRow(ExcelWorksheet worksheet, int rowNumber);
    }
}
