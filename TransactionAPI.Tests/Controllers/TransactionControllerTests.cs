using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Security.Claims;
using TransactionAPI.Controllers;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Transactions;

namespace TransactionAPI.Tests.Controllers
{
    public class TransactionControllerTests
    {
        private TransactionController _transactionController;
        private Mock<ITransactionService> _transactionServiceMock;
        private Mock<ILogger<TransactionController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            this._transactionServiceMock = new Mock<ITransactionService>();
            this._loggerMock = new Mock<ILogger<TransactionController>>();

            this._transactionController = new TransactionController(_transactionServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetAll_TransactionsNotFound_ReturnsNotFound()
        {
            // Arrange
            var emptyTransactionList = new List<Transaction>();
            var filter = new TransactionFilterViewModel()
            {
                ClientName = "Some text",
                Status = Status.Pending,
                Types = new List<Domain.Enums.Type>() { Domain.Enums.Type.Withdrawal }
            };

            _transactionServiceMock.Setup(service => service.GetTransactionsByFilter(It.IsAny<List<Domain.Enums.Type>>(), It.IsAny<Status>(), It.IsAny<string>())).ReturnsAsync(emptyTransactionList);

            // Act
            var actualResult = await _transactionController.GetAll(filter);
            var notFoundResult = actualResult as NotFoundObjectResult;

            // Assert
            Assert.NotNull(notFoundResult);
            Assert.AreEqual((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);
            Assert.AreEqual(notFoundResult.Value, "Transactions not found.");
        }

        [Test]
        public async Task GetAll_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var exception = new Exception("Some error message");
            var filter = new TransactionFilterViewModel()
            {
                ClientName = "Some text",
                Status = Status.Pending,
                Types = new List<Domain.Enums.Type>() { Domain.Enums.Type.Withdrawal }
            };

            _transactionServiceMock.Setup(service => service.GetTransactionsByFilter(It.IsAny<List<Domain.Enums.Type>>(), It.IsAny<Status>(), It.IsAny<string>())).ThrowsAsync(exception);

            // Act
            var actualResult = await _transactionController.GetAll(filter);
            var internalServerResult = actualResult as ObjectResult;

            // Assert
            Assert.NotNull(internalServerResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerResult.StatusCode);
            Assert.AreEqual(internalServerResult.Value, exception.Message);
        }

        [Test]
        public async Task GetAll_CorrectTransactions_ReturnsOkWithResult()
        {
            // Arrange
            var expectedTransactions = new List<Transaction>()
            {
                new Transaction{ Amount = 22, Type = Domain.Enums.Type.Withdrawal, ClientName = "Some text", Status = Status.Pending, TransactionId = 1 },
                new Transaction{ Amount = 13, Type = Domain.Enums.Type.Refill, ClientName = "Some text", Status = Status.Pending, TransactionId = 2 }
            };
            var filter = new TransactionFilterViewModel()
            {
                ClientName = "Some text",
                Status = Status.Pending,
                Types = null
            };

            _transactionServiceMock.Setup(service => service.GetTransactionsByFilter(It.IsAny<List<Domain.Enums.Type>>(), It.IsAny<Status>(), It.IsAny<string>())).ReturnsAsync(expectedTransactions);

            // Act
            var actualResult = await _transactionController.GetAll(filter);
            var okResult = actualResult as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);
            CollectionAssert.AreEqual((List<Transaction>)okResult.Value, expectedTransactions);
        }

        [Test]
        public async Task UpdateTransactionStatusById_StatusNull_ReturnsBadRequest()
        {
            // Arrange & Act
            var actualResult = await _transactionController.UpdateTransactionStatusById(It.IsAny<int>(), null);
            var badRequestResult = actualResult as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(badRequestResult.Value, "Status is required.");
        }

        [Test]
        public async Task UpdateTransactionStatusById_TransactionNotFound_ReturnsNotFound()
        {
            // Arrange
            Transaction transaction = null;

            _transactionServiceMock.Setup(service => service.GetTransactionById(It.IsAny<int>())).ReturnsAsync(transaction);

            // Act
            var actualResult = await _transactionController.UpdateTransactionStatusById(It.IsAny<int>(), It.IsAny<Status>());
            var notFoundResult = actualResult as NotFoundObjectResult;

            // Assert
            Assert.NotNull(notFoundResult);
            Assert.AreEqual((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);
            Assert.AreEqual(notFoundResult.Value, "Transaction not found.");
        }

        [Test]
        public async Task UpdateTransactionStatusById_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var exception = new Exception("Some error message");
            var status = Status.Completed;

            _transactionServiceMock.Setup(service => service.GetTransactionById(It.IsAny<int>())).ThrowsAsync(exception);

            // Act
            var actualResult = await _transactionController.UpdateTransactionStatusById(It.IsAny<int>(), status);
            var internalServerResult = actualResult as ObjectResult;

            // Assert
            Assert.NotNull(internalServerResult);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, internalServerResult.StatusCode);
            Assert.AreEqual(internalServerResult.Value, exception.Message);
        }

        [Test]
        public async Task UpdateTransactionStatusById_CorrectTransactionAndStatus_ReturnsOkWithResult()
        {
            // Arrange
            var transaction = new Transaction { Amount = 22, Type = Domain.Enums.Type.Withdrawal, ClientName = "Some text", Status = Status.Pending, TransactionId = 1 };
            var status = Status.Completed;
            var expectedTransaction = new Transaction { Amount = 22, Type = Domain.Enums.Type.Withdrawal, ClientName = "Some text", Status = status, TransactionId = 1 };

            _transactionServiceMock.Setup(service => service.GetTransactionById(transaction.TransactionId)).ReturnsAsync(transaction);
            _transactionServiceMock.Setup(service => service.UpdateTransactionStatus(transaction, status)).ReturnsAsync(expectedTransaction);

            // Act
            var actualResult = await _transactionController.UpdateTransactionStatusById(transaction.TransactionId, status);
            var okResult = actualResult as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.AreEqual((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.AreEqual(okResult.Value, expectedTransaction);
            Assert.AreNotEqual(okResult.Value, transaction);
        }
    }
}
