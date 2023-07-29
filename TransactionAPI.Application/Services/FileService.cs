using System.Globalization;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Transactions;

namespace TransactionAPI.Application.Services
{
    public class FileService : IFileService
    {
        private readonly ITransactionService _transactionService;

        public FileService(ITransactionService transactionService)
        {
            this._transactionService = transactionService;
        }

        public async Task ProcessExcelFile(Stream fileStream)
        {
            fileStream.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(fileStream))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] values = line.Split(',');
                        if (values.Length >= 5 &&
                            int.TryParse(values[0], out int transactionId) &&
                            Enum.TryParse(values[1], out Status status) &&
                            Enum.TryParse(values[2], out Domain.Enums.Type type) &&
                            decimal.TryParse(values[4], NumberStyles.Currency, new CultureInfo("en-US"), out decimal amount))
                        {
                            var transaction = new Transaction
                            {
                                TransactionId = transactionId,
                                Status = status,
                                Type = type,
                                ClientName = values[3],
                                Amount = amount
                            };

                            var existingTransaction = await _transactionService.GetTransactionById(transaction.TransactionId);

                            if (existingTransaction != null)
                            {
                                await _transactionService.UpdateTransactionStatus(existingTransaction, transaction.Status);
                            }
                            else
                            {
                                await _transactionService.AddTransactionToDatabase(transaction);
                            }
                        }
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
