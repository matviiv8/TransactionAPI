using TransactionAPI.Infrastructure.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using TransactionAPI.Domain.Models;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Transactions;

namespace TransactionAPI.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly TransactionAPIDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public TransactionService(TransactionAPIDbContext dbContext, IConfiguration configuration)
        {
            this._dbContext = dbContext;
            this._configuration = configuration;
        }

        public async Task<List<Transaction>> GetTransactionsByFilter(List<Domain.Enums.Type>? types, Status? status, string? clientName = null)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT * FROM Transactions WHERE 1=1";

                if (types != null && types.Any())
                {
                    string typeList = string.Join(",", types.Select(t => $"'{t}'"));
                    query += $" AND Type IN ({typeList})";
                }

                if (status != null)
                {
                    query += $" AND Status = '{status}'";
                }

                if (!string.IsNullOrEmpty(clientName))
                {
                    query += $" AND ClientName LIKE '%{clientName}%'";
                }

                SqlCommand command = new SqlCommand(query, connection);

                await connection.OpenAsync();

                SqlDataReader dataReader = command.ExecuteReader();

                var transactions = new List<Transaction>();

                while (await dataReader.ReadAsync())
                {
                    transactions.Add(await ReadTransaction(dataReader));
                }

                return transactions;
            }
        }

        public async Task<Transaction> GetTransactionById(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"SELECT * 
                                 FROM Transactions
                                 Where Transactions.TransactionId = @id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();

                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.HasRows)
                {
                    await dataReader.ReadAsync();

                    return await ReadTransaction(dataReader);
                }

                return null;
            }
        }

        public async Task<Transaction> UpdateTransactionStatus(Transaction transaction, Status newStatus)
        {
            transaction.Status = newStatus;

            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();

            return transaction;
        }

        public async Task AddTransactionToDatabase(Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<Transaction> ReadTransaction(SqlDataReader dataReader)
        {
            int idIndex = dataReader.GetOrdinal("TransactionId");
            int clientNameIndex = dataReader.GetOrdinal("ClientName");
            int statusIndex = dataReader.GetOrdinal("Status");
            int typeIndex = dataReader.GetOrdinal("Type");
            int amountIndex = dataReader.GetOrdinal("Amount");

            Transaction transaction = new Transaction
            {
                TransactionId = await dataReader.GetFieldValueAsync<int>(idIndex),
                ClientName = await dataReader.GetFieldValueAsync<string>(clientNameIndex),
                Status = (Status)Enum.Parse(typeof(Status), await dataReader.GetFieldValueAsync<string>(statusIndex)),
                Type = (Domain.Enums.Type)Enum.Parse(typeof(Domain.Enums.Type), await dataReader.GetFieldValueAsync<string>(typeIndex)),
                Amount = await dataReader.GetFieldValueAsync<decimal>(amountIndex)
            };

            return transaction;
        }
    }
}