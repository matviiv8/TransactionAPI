using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Application.Services;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces;

namespace TransactionAPI.Tests.Services
{
    public class FileServiceTests
    {
        private Mock<ITransactionService> _transactionServiceMock;
        private FileService _fileService;
        /*
        [SetUp]
        public void Setup()
        {
            this._transactionServiceMock = new Mock<ITransactionService>();
            this._fileService = new FileService(_transactionServiceMock.Object);
        }

      
        [Test]
        public async Task ProcessExcelFile_ValidData_AddsTransactionsToDatabase()
        {
            // Arrange
            string fileContent = "TransactionId,Status,Type,ClientName,Amount\n" +
                                                   "1,Completed,Withdrawal,John,100\n" +
                                                   "2,Cancelled,Refill,Jane,200\n";
            Stream fileStream = GenerateStreamFromString(fileContent);

            // Act
            await _fileService.ProcessExcelFile(fileStream);

            // Assert
            zz_transactionServiceMock.Verify(service => service.AddTransactionToDatabase(It.IsAny<Transaction>()), Times.Exactly(2));
        }

        [Test]
        public async Task ProcessExcelFile_DuplicateTransaction_UpdatesTransactionStatus()
        {
            // Arrange
            string fileContent = "TransactionId,Status,Type,ClientName,Amount\n" +
                                        "1,Completed,Withdrawal,John,100\n";
            Stream fileStream = GenerateStreamFromString(fileContent);
            var existingTransaction = new Transaction
            {
                TransactionId = 1,
                Status = Status.Cancelled,
                Type = Domain.Enums.Type.Refill,
                ClientName = "John",
                Amount = 200
            };

            _transactionServiceMock.Setup(service => service.GetTransactionById(1)).ReturnsAsync(existingTransaction);

            // Act
            await _fileService.ProcessExcelFile(fileStream);

            // Assert
            _transactionServiceMock.Verify(service => service.UpdateTransactionStatus(existingTransaction, Status.Completed), Times.Once);
            _transactionServiceMock.Verify(service => service.AddTransactionToDatabase(It.IsAny<Transaction>()), Times.Never);
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

        private Stream GenerateStreamFromString(string content)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
        */
    }
}
