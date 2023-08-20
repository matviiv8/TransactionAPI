using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Application.Services.Transactions;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Context;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Tests.Services.Transactions
{
    public class TransactionServiceTests
    {
        private TransactionService _transactionService;
        private TransactionAPIDbContext _dbContext;
        private IConfiguration _configuration;
        private Mock<ILogger<TransactionService>> _loggerMock;
        
        [SetUp]
        public void Setup()
        {
            this._configuration = new ConfigurationBuilder()
                 .AddInMemoryCollection(new Dictionary<string, string>
                 {
                    {"ConnectionStrings:DefaultConnection", "Server=(localdb)\\MSSQLLocalDB;Database=TestDatabase;Trusted_Connection=True;"}
                 })
                 .Build();

            var options = new DbContextOptionsBuilder<TransactionAPIDbContext>()
                    .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
                    .Options;
            this._dbContext = new TransactionAPIDbContext(options);
            this._loggerMock = new Mock<ILogger<TransactionService>>();

            this._transactionService = new TransactionService(_dbContext, _configuration, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this._dbContext.Database.EnsureDeleted();
        }

        [Test]
        public async Task UpdateTransactionStatus_DatabaseError_ThrowsApplicationException()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = Status.Pending,
                Type = Type.Withdrawal,
                ClientName = "John",
                Amount = 100
            };
            _dbContext.Database.EnsureDeleted();

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () =>
                await _transactionService.UpdateTransactionStatus(transaction, Status.Completed));
        }

        [Test]
        public async Task UpdateTransactionStatus_ValidTransaction_ReturnsUpdatedTransaction()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = Status.Pending,
                Type = Type.Withdrawal,
                ClientName = "John",
                Amount = 100
            };

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var newStatus = Status.Completed;
            var updatedTransaction = await _transactionService.UpdateTransactionStatus(transaction, newStatus);
            var transactionFromDb = await _dbContext.Transactions.FindAsync(1);

            // Assert
            Assert.IsNotNull(updatedTransaction);
            Assert.AreEqual(newStatus, updatedTransaction.Status);
            Assert.AreEqual(newStatus, transactionFromDb.Status);
        }

        [Test]
        public async Task GetTransactionById_ExistingTransactionId_ReturnsTransaction()
        {
            // Arrange
            var transaction = new Transaction
            {
                Status = Status.Completed,
                Type = Type.Withdrawal,
                ClientName = "John",
                Amount = 100
            };

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionService.GetTransactionById(transaction.TransactionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transaction.TransactionId, result.TransactionId);
            Assert.AreEqual(transaction.Status, result.Status);
            Assert.AreEqual(transaction.Type, result.Type);
            Assert.AreEqual(transaction.ClientName, result.ClientName);
            Assert.AreEqual(transaction.Amount, result.Amount);
        }

        [Test]
        public async Task GetTransactionById_DatabaseError_ThrowsApplicationException()
        {
            // Arrange
            _dbContext.Database.EnsureDeleted();

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () =>
                await _transactionService.GetTransactionById(It.IsAny<int>()));
        }

        [Test]
        public async Task GetTransactionById_NotExistingTransactionId_ReturnsNull()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = Status.Completed, Type = Type.Withdrawal, ClientName = "John", Amount = 100 },
                new Transaction { TransactionId = 2, Status = Status.Cancelled, Type = Type.Refill, ClientName = "Jane", Amount = 200 }
            };

            await _dbContext.Transactions.AddRangeAsync(transactions);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionService.GetTransactionById(3);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetTransactionsByFilter_ValidData_ReturnsFilteredTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = Status.Completed, Type = Type.Withdrawal, ClientName = "John", Amount = 100 },
                new Transaction { TransactionId = 2, Status = Status.Cancelled, Type = Type.Refill, ClientName = "Jane", Amount = 200 }
            };

            await _dbContext.Transactions.AddRangeAsync(transactions);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionService.GetTransactionsByFilter(new List<Type> { Type.Refill }, Status.Cancelled, "Jane");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].TransactionId);
            Assert.AreEqual("Jane", result[0].ClientName);
            Assert.AreEqual(Status.Cancelled, result[0].Status);
            Assert.AreEqual(Type.Refill, result[0].Type);
            Assert.AreEqual(200, result[0].Amount);
        }

        [Test]
        public async Task GetTransactionsByFilter_AllParametersNull_ReturnsAllTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = Status.Completed, Type = Type.Withdrawal, ClientName = "John", Amount = 100 },
                new Transaction { TransactionId = 2, Status = Status.Cancelled, Type = Type.Refill, ClientName = "Jane", Amount = 200 }
            };

            await _dbContext.Transactions.AddRangeAsync(transactions);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionService.GetTransactionsByFilter(null, null, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transactions.Count, result.Count);

            foreach (var expectedTransaction in transactions)
            {
                Assert.IsTrue(result.Any(transaction =>
                    transaction.TransactionId == expectedTransaction.TransactionId &&
                    transaction.Status == expectedTransaction.Status &&
                    transaction.Type == expectedTransaction.Type &&
                    transaction.ClientName == expectedTransaction.ClientName &&
                    transaction.Amount == expectedTransaction.Amount
                ));
            }
        }

        [Test]
        public async Task GetTransactionsByFilter_DatabaseError_ThrowsApplicationException()
        {
            // Arrange
            _dbContext.Database.EnsureDeleted();

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () =>
                await _transactionService.GetTransactionsByFilter(null, null, null));
        }

        [Test]
        public async Task MergeTransaction_DatabaseError_ThrowsApplicationException()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = Status.Pending,
                Type = Type.Withdrawal,
                ClientName = "John",
                Amount = 100
            };
            _dbContext.Database.EnsureDeleted();

            // Act & Assert
            Assert.ThrowsAsync<ApplicationException>(async () =>
                await _transactionService.MergeTransaction(transaction));
        }

        [Test]
        public async Task MergeTransaction_SuccessfullyMergesTransaction()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = Status.Completed,
                Type = Type.Withdrawal,
                ClientName = "John",
                Amount = 100
            };

            // Act
            await _transactionService.MergeTransaction(transaction);

            // Assert
            var mergedTransaction = await _dbContext.Transactions.FindAsync(1);
            Assert.IsNotNull(mergedTransaction);
            Assert.AreEqual(Status.Completed, mergedTransaction.Status);
            Assert.AreEqual(Type.Withdrawal, mergedTransaction.Type);
            Assert.AreEqual("John", mergedTransaction.ClientName);
            Assert.AreEqual(100, mergedTransaction.Amount);
        }
    }
}
