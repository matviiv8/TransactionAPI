using OfficeOpenXml;
using TransactionAPI.Domain.Models;

namespace TransactionAPI.Infrastructure.Interfaces
{
    public interface ITransactionParsingService
    {
        Transaction ParseTransactionRow(ExcelWorksheet worksheet, int rowNumber);
    }
}
