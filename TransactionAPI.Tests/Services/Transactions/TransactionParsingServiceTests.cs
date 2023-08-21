using Moq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Application.Services.Transactions;
using TransactionAPI.Domain.Models;

namespace TransactionAPI.Tests.Services.Transactions
{
    public class TransactionParsingServiceTests
    {
        private TransactionParsingService _transactionParsingService;
        private ExcelPackage _excelPackage;

        [SetUp]
        public void Setup()
        {
            this._excelPackage = new ExcelPackage();

            this._transactionParsingService = new TransactionParsingService();
        }

        [Test]
        public void ParseTransactionRow_ValidData_ReturnsTransaction()
        {
            // Arrange
            var worksheet = _excelPackage.Workbook.Worksheets.Add("Sheet1");
            worksheet.Cells["A1"].Value = 1;
            worksheet.Cells["B1"].Value = "Cancelled";
            worksheet.Cells["C1"].Value = "Refill";
            worksheet.Cells["D1"].Value = "John";
            worksheet.Cells["E1"].Value = "$200";

            // Act
            var transaction = _transactionParsingService.ParseTransactionRow(worksheet, 1);

            // Assert
            Assert.NotNull(transaction);
            Assert.IsInstanceOf<Transaction>(transaction);
        }

        [Test]
        public void ParseTransactionRow_InvalidData_ReturnsNull()
        {
            // Arrange
            var worksheet = _excelPackage.Workbook.Worksheets.Add("Sheet1");
            worksheet.Cells["A1"].Value = "InvalidValue";

            // Act
            var transaction = _transactionParsingService.ParseTransactionRow(worksheet, 1);

            // Assert
            Assert.Null(transaction);
        }
    }
}
