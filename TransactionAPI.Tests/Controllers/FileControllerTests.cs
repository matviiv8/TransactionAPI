using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TransactionAPI.Controllers;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Files;
using TransactionAPI.Infrastructure.Interfaces.Transactions;
using TransactionAPI.Infrastructure.ViewModels.Transactions;

namespace TransactionAPI.Tests.Controllers
{
    public class FileControllerTests
    {
        private FileController _fileController;
        private Mock<IFileService> _fileServiceMock;
        private Mock<ITransactionService> _transactionServiceMock;
        private Mock<ILogger<FileController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            this._fileServiceMock = new Mock<IFileService>();
            this._transactionServiceMock = new Mock<ITransactionService>();
            this._loggerMock = new Mock<ILogger<FileController>>();

            this._fileController = new FileController(_fileServiceMock.Object, _transactionServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task ProcessExcelFile_ValidCsvFile_ReturnsOk()
        {
            // Arrange
            var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("Test CSV Data")), 0, 0, "Data", "data.xlsx");

            // Act
            var actualResult = await _fileController.ProcessExcelFile(file);
            var okResult = actualResult as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.AreEqual("The file has been read and inserted into the database.", okResult.Value);
            _fileServiceMock.Verify(service => service.ProcessExcelFile(It.IsAny<Stream>()), Times.Once);
        }

        [Test]
        public async Task ProcessExcelFile_InvalidFileFormat_ReturnsBadRequest()
        {
            // Arrange
            var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("Test Data")), 0, 0, "Data", "data.txt");

            // Act
            var actualResult = await _fileController.ProcessExcelFile(file);
            var badRequestResult = actualResult as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual("The file format is incorrect.", badRequestResult.Value);
        }

        [Test]
        public async Task ProcessExcelFile_InternalServerError_ReturnsInternalServerError()
        {
            // Arrange
            var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("Test Data")), 0, 0, "Data", "data.xlsx");
            var exception = new Exception("Some error message");

            _fileServiceMock.Setup(service => service.ProcessExcelFile(It.IsAny<Stream>())).ThrowsAsync(exception);

            // Act
            var actualResult = await _fileController.ProcessExcelFile(file);
            var internalServerErrorResult = actualResult as ObjectResult;

            // Assert
            Assert.NotNull(internalServerErrorResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerErrorResult.StatusCode);
            Assert.AreEqual("Some error message", internalServerErrorResult.Value);
        }

        [Test]
        public async Task ExportTransactionsToCsv_ValidFilter_ReturnsOk()
        {
            // Arrange
            var filter = new TransactionTypeStatusViewModel
            {
                Type = Domain.Enums.Type.Withdrawal,
                Status = Status.Pending
            };
            var expectedTransactions = new List<Transaction>()
            {
                new Transaction{ Amount = 22, Type = Domain.Enums.Type.Withdrawal, ClientName = "Some text", Status = Status.Pending, TransactionId = 1 },
                new Transaction{ Amount = 13, Type = Domain.Enums.Type.Refill, ClientName = "Some text", Status = Status.Pending, TransactionId = 2 }
            };

            _transactionServiceMock.Setup(service => service.GetTransactionsByFilter(It.IsAny<List<Domain.Enums.Type>>(), It.IsAny<Status>(), It.IsAny<string>())).ReturnsAsync(expectedTransactions);

            // Act
            var actualResult = await _fileController.ExportTransactionsToCsv(filter);
            var okResult = actualResult as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.AreEqual("Export completed successfully. Data saved to file transactions.csv", okResult.Value);
            _fileServiceMock.Verify(service => service.ExportTransactionsToCsv(It.IsAny<List<Transaction>>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ExportTransactionsToCsv_NoTransactionsFound_ReturnsNotFound()
        {
            // Arrange
            var emptyTransactionList = new List<Transaction>();
            var filter = new TransactionTypeStatusViewModel
            {
                Type = Domain.Enums.Type.Withdrawal,
                Status = Status.Pending
            };

            _transactionServiceMock.Setup(service => service.GetTransactionsByFilter(It.IsAny<List<Domain.Enums.Type>>(), It.IsAny<Status>(), It.IsAny<string>())).ReturnsAsync(emptyTransactionList);

            // Act
            var actualResult = await _fileController.ExportTransactionsToCsv(filter);
            var notFoundResult = actualResult as NotFoundObjectResult;

            // Assert
            Assert.NotNull(notFoundResult);
            Assert.AreEqual((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);
            Assert.AreEqual("Transactions not found.", notFoundResult.Value);
        }

        [Test]
        public async Task ExportTransactionsToCsv_InternalServerError_ReturnsInternalServerError()
        {
            // Arrange
            var filter = new TransactionTypeStatusViewModel
            {
                Type = Domain.Enums.Type.Withdrawal,
                Status = Status.Pending
            };
            var exception = new Exception("Some error message");

            _transactionServiceMock.Setup(service => service.GetTransactionsByFilter(It.IsAny<List<Domain.Enums.Type>>(), It.IsAny<Status>(), It.IsAny<string>())).ThrowsAsync(exception);

            // Act
            var actualResult = await _fileController.ExportTransactionsToCsv(filter);
            var internalServerErrorResult = actualResult as ObjectResult;

            // Assert
            Assert.NotNull(internalServerErrorResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerErrorResult.StatusCode);
            Assert.AreEqual("Some error message", internalServerErrorResult.Value);
        }
    }
}
