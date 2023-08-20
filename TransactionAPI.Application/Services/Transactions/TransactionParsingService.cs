using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Transactions;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Application.Services.Transactions
{
    public class TransactionParsingService : ITransactionParsingService
    {
        private bool TryParseInt(ExcelWorksheet worksheet, int rowNumber, int columnNumber, out int result)
        {
            return int.TryParse(worksheet.Cells[rowNumber, columnNumber].Text, out result);
        }

        private bool TryParseEnum<TEnum>(ExcelWorksheet worksheet, int rowNumber, int columnNumber, out TEnum result) where TEnum : struct
        {
            return Enum.TryParse(worksheet.Cells[rowNumber, columnNumber].Text, out result);
        }

        private bool TryParseDecimal(ExcelWorksheet worksheet, int rowNumber, int columnNumber, out decimal result)
        {
            return decimal.TryParse(worksheet.Cells[rowNumber, columnNumber].Text, NumberStyles.Currency, new CultureInfo("en-US"), out result);
        }

        public Transaction ParseTransactionRow(ExcelWorksheet worksheet, int rowNumber)
        {
            int transactionId;
            Status status;
            Type type;
            string clientName;
            decimal amount;

            if (!TryParseInt(worksheet, rowNumber, 1, out transactionId) ||
                !TryParseEnum(worksheet, rowNumber, 2, out status) ||
                !TryParseEnum(worksheet, rowNumber, 3, out type) ||
                !TryParseDecimal(worksheet, rowNumber, 5, out amount))
            {
                return null;
            }

            clientName = worksheet.Cells[rowNumber, 4].Text;

            var transaction = new Transaction
            {
                TransactionId = transactionId,
                Status = status,
                Type = type,
                ClientName = clientName,
                Amount = amount
            };

            return transaction;
        }
    }
}
