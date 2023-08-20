using Microsoft.Extensions.Logging;
using Moq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Application.Services.Files;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Transactions;

namespace TransactionAPI.Tests.Services
{
    public class FileServiceTests
    {
        private Mock<ITransactionService> _transactionServiceMock;
        private Mock<ITransactionParsingService> _transactionParsingServiceMock;
        private Mock<ILogger<FileService>> _loggerMock;
        private FileService _fileService;

        [SetUp]
        public void Setup()
        {
            this._transactionServiceMock = new Mock<ITransactionService>();
            this._transactionParsingServiceMock = new Mock<ITransactionParsingService>();
            this._loggerMock = new Mock<ILogger<FileService>>();

            this._fileService = new FileService(_transactionServiceMock.Object, _transactionParsingServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task ExportTransactionsToCsv_ValidData_WritesToFile()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = Status.Completed, Type = Domain.Enums.Type.Withdrawal, ClientName = "John", Amount = 100 },
                new Transaction { TransactionId = 2, Status = Status.Cancelled, Type = Domain.Enums.Type.Refill, ClientName = "Jane", Amount = 200 }
            };
            string expectedCsvContent = "TransactionId,Status,Type,ClientName,Amount\n" +
                                        "1,Completed,Withdrawal,John,100\n" +
                                        "2,Cancelled,Refill,Jane,200\n";
            string filePath = "transactions.csv";

            // Act
            await _fileService.ExportTransactionsToCsv(transactions, filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));

            string actualCsvContent;
            using (var reader = new StreamReader(filePath))
            {
                actualCsvContent = reader.ReadToEnd();
            }
            expectedCsvContent = expectedCsvContent.Replace("\r\n", "\n");
            actualCsvContent = actualCsvContent.Replace("\r\n", "\n");

            Assert.AreEqual(expectedCsvContent, actualCsvContent);

            File.Delete(filePath);
        }
    }
}
