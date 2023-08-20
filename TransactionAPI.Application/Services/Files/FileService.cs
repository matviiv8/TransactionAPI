using System.Globalization;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using OfficeOpenXml;
using Type = TransactionAPI.Domain.Enums.Type;
using TransactionAPI.Infrastructure.Interfaces.Files;
using TransactionAPI.Infrastructure.Interfaces.Transactions;
using Microsoft.Extensions.Logging;

namespace TransactionAPI.Application.Services.Files
{
    public class FileService : IFileService
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionParsingService _transactionParsingService;
        private readonly ILogger<FileService> _logger;

        public FileService(ITransactionService transactionService, ITransactionParsingService transactionParsingService, ILogger<FileService> logger)
        {
            this._transactionService = transactionService;
            this._transactionParsingService = transactionParsingService;
            this._logger = logger;
        }

        public async Task ProcessExcelFile(Stream fileStream)
        {
            try
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
            catch (Exception exception)
            {
                _logger.LogError($"Error in FileService.ProcessExcelFile(fileStream): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while processing Excel file.", exception);
            }
        }

        public async Task ExportTransactionsToCsv(List<Transaction> transactions, string filePath)
        {
            try
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
            catch (Exception exception)
            {
                _logger.LogError($"Error in FileService.ExportTransactionsToCsv(transactions, filePath): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while exporting transactions to CSV.", exception);
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
