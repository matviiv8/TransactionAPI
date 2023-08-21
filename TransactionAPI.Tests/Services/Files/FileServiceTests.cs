using Microsoft.Extensions.Logging;
using Moq;
using OfficeOpenXml;
using TransactionAPI.Application.Services.Files;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Transactions;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Tests.Services.Files
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
                new Transaction { TransactionId = 1, Status = Status.Completed, Type = Type.Withdrawal, ClientName = "John", Amount = 100 },
                new Transaction { TransactionId = 2, Status = Status.Cancelled, Type = Type.Refill, ClientName = "Jane", Amount = 200 }
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

        [Test]
        public async Task ExportTransactionsToCsv_ExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            var transactions = new List<Transaction>();
            string filePath = null;

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _fileService.ExportTransactionsToCsv(transactions, filePath));

            Assert.AreEqual("Error while exporting transactions to CSV.", exception.Message);
        }

        [Test]
        public async Task ProcessExcelFile_ValidData_ProcessesSuccessfully()
        {
            // Arrange
            var fileStream = new MemoryStream();
            var transaction = new Transaction { TransactionId = 1, Status = Status.Cancelled, Type = Type.Refill, ClientName = "John", Amount = 200 };

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].Value = "TransactionId";
                worksheet.Cells["B1"].Value = "Status";
                worksheet.Cells["C1"].Value = "Type";
                worksheet.Cells["D1"].Value = "ClientName";
                worksheet.Cells["E1"].Value = "Amount";
                worksheet.Cells["A2"].Value = 1;
                worksheet.Cells["B2"].Value = "Cancelled";
                worksheet.Cells["C2"].Value = "Refill";
                worksheet.Cells["D2"].Value = "John";
                worksheet.Cells["E2"].Value = "$200";
                package.Save();
                fileStream.Position = 0;
            }

            _transactionParsingServiceMock.Setup(service => service.ParseTransactionRow(It.IsAny<ExcelWorksheet>(), It.IsAny<int>())).Returns(transaction);
            // Act & Assert
            Assert.DoesNotThrowAsync(async () =>
                await _fileService.ProcessExcelFile(fileStream));
            _transactionServiceMock.Verify(service => service.MergeTransaction(It.IsAny<Transaction>()), Times.Once);
        }

        [Test]
        public async Task ProcessExcelFile_ExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            var fileStream = new MemoryStream();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _fileService.ProcessExcelFile(fileStream));

            Assert.NotNull(exception.InnerException);
            Assert.IsInstanceOf<Exception>(exception.InnerException);
            Assert.AreEqual("Error while processing Excel file.", exception.Message);
        }
    }
}
