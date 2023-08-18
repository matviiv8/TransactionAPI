using System.Globalization;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces;
using OfficeOpenXml;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Application.Services
{
    public class FileService : IFileService
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionParsingService _transactionParsingService;

        public FileService(ITransactionService transactionService, ITransactionParsingService transactionParsingService)
        {
            this._transactionService = transactionService;
            this._transactionParsingService = transactionParsingService;
        }

        public async Task ProcessExcelFile(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];

                int startRow = 2;

                for (int row = startRow; row <= worksheet.Dimension.End.Row; row++)
                {
                    var transaction = _transactionParsingService.ParseTransactionRow(worksheet, row);

                    if (transaction != null)
                    {
                        await _transactionService.MergeTransaction(transaction);
                    }
                }
            }
        }

        public async Task ExportTransactionsToCsv(List<Transaction> transactions, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteLineAsync("TransactionId,Status,Type,ClientName,Amount");

                foreach (var transaction in transactions)
                {
                    string transactionId = EscapeCsvField(transaction.TransactionId.ToString());
                    string status = EscapeCsvField(transaction.Status.ToString());
                    string type = EscapeCsvField(transaction.Type.ToString());
                    string clientName = EscapeCsvField(transaction.ClientName);
                    string amount = EscapeCsvField(transaction.Amount.ToString());

                    await writer.WriteLineAsync($"{transactionId},{status},{type},{clientName},{amount}");
                }
            }
        }

        private string EscapeCsvField(string field)
        {
            if (field.Contains(',') || field.Contains('"'))
            {
                field = field.Replace("\"", "\"\"");
                field = $"\"{field}\"";
            }

            return field;
        }
    }
}
