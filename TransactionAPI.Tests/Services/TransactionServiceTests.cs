using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Application.Services;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Context;

namespace TransactionAPI.Tests.Services
{
    public class TransactionServiceTests
    {
        private TransactionService _transactionService;
        private TransactionAPIDbContext _dbContext;
        private IConfiguration _configuration;

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

            this._transactionService = new TransactionService(_dbContext, _configuration);
        }

        [TearDown]
        public void TearDown()
        {
            this._dbContext.Database.EnsureDeleted();
        }

        [Test]
        public async Task AddTransactionToDatabase_ValidTransaction_AddsTransactionToDatabase()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = Status.Pending,
                Type = Domain.Enums.Type.Withdrawal,
                ClientName = "John",
                Amount = 100
            };

            // Act
            await _transactionService.AddTransactionToDatabase(transaction);
            var transactionFromDb = await _dbContext.Transactions.FindAsync(1);

            // Assert
            Assert.IsNotNull(transactionFromDb);
            Assert.AreEqual(transaction.Status, transactionFromDb.Status);
        }

        [Test]
        public async Task UpdateTransactionStatus_ValidTransaction_ReturnsUpdatedTransaction()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 1,
                Status = Status.Pending,
                Type = Domain.Enums.Type.Withdrawal,
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
                TransactionId = 1,
                Status = Status.Completed,
                Type = Domain.Enums.Type.Withdrawal,
                ClientName = "John",
                Amount = 100
            };

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionService.GetTransactionById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transaction.TransactionId, result.TransactionId);
            Assert.AreEqual(transaction.Status, result.Status);
            Assert.AreEqual(transaction.Type, result.Type);
            Assert.AreEqual(transaction.ClientName, result.ClientName);
            Assert.AreEqual(transaction.Amount, result.Amount);
        }

        [Test]
        public async Task GetTransactionById_NotExistingTransactionId_ReturnsNull()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = Status.Completed, Type = Domain.Enums.Type.Withdrawal, ClientName = "John", Amount = 100 },
                new Transaction { TransactionId = 2, Status = Status.Cancelled, Type = Domain.Enums.Type.Refill, ClientName = "Jane", Amount = 200 }
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
                new Transaction { TransactionId = 1, Status = Status.Completed, Type = Domain.Enums.Type.Withdrawal, ClientName = "John", Amount = 100 },
                new Transaction { TransactionId = 2, Status = Status.Cancelled, Type = Domain.Enums.Type.Refill, ClientName = "Jane", Amount = 200 }
            };

            await _dbContext.Transactions.AddRangeAsync(transactions);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _transactionService.GetTransactionsByFilter(new List<Domain.Enums.Type> { Domain.Enums.Type.Refill }, Status.Cancelled, "Jane");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].TransactionId);
            Assert.AreEqual("Jane", result[0].ClientName);
            Assert.AreEqual(Status.Cancelled, result[0].Status);
            Assert.AreEqual(Domain.Enums.Type.Refill, result[0].Type);
            Assert.AreEqual(200, result[0].Amount);
        }

        [Test]
        public async Task GetTransactionsByFilter_AllParametersNull_ReturnsAllTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, Status = Status.Completed, Type = Domain.Enums.Type.Withdrawal, ClientName = "John", Amount = 100 },
                new Transaction { TransactionId = 2, Status = Status.Cancelled, Type = Domain.Enums.Type.Refill, ClientName = "Jane", Amount = 200 }
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
    }
}
