using TransactionAPI.Infrastructure.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using TransactionAPI.Domain.Models;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Transactions;
using System.Text;
using Type = TransactionAPI.Domain.Enums.Type;

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

        public async Task<List<Transaction>> GetTransactionsByFilter(List<Type>? types, Status? status, string? clientName = null)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = BuildQueryWithFilters(types, status, clientName);
                SqlCommand command = CreateSqlCommandWithParameters(query, types, status, clientName);

                await connection.OpenAsync();

                SqlDataReader dataReader = await command.ExecuteReaderAsync();

                var transactions = new List<Transaction>();

                while (await dataReader.ReadAsync())
                {
                    transactions.Add(await ReadTransaction(dataReader));
                }

                return transactions;
            }
        }

        private string BuildQueryWithFilters(List<Type>? types, Status? status, string? clientName)
        {
            StringBuilder queryBuilder = new StringBuilder("SELECT * FROM Transactions WHERE 1=1");

            if (types != null && types.Any())
            {
                string typeList = string.Join(",", types.Select(t => $"'{t}'"));
                queryBuilder.Append($" AND Type IN ({typeList})");
            }

            if (status != null)
            {
                queryBuilder.Append(" AND Status = @status");
            }

            if (!string.IsNullOrEmpty(clientName))
            {
                queryBuilder.Append(" AND ClientName LIKE @clientName");
            }

            return queryBuilder.ToString();
        }

        private SqlCommand CreateSqlCommandWithParameters(string query, List<Type>? types, Status? status, string? clientName)
        {
            SqlCommand command = new SqlCommand(query);

            if (status != null)
            {
                command.Parameters.AddWithValue("@status", status);
            }

            if (!string.IsNullOrEmpty(clientName))
            {
                command.Parameters.AddWithValue("@clientName", "%" + clientName + "%");
            }

            return command;
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

        public async Task MergeTransaction(Transaction transaction)
        {
            string mergeQuery = @"
                MERGE INTO Transactions AS target
                USING (SELECT @TransactionId AS TransactionId) AS source
                ON target.TransactionId = source.TransactionId
                WHEN MATCHED THEN
                    UPDATE SET 
                        target.Status = @Status
                WHEN NOT MATCHED THEN
                    INSERT (TransactionId, ClientName, Status, Type, Amount)
                    VALUES (@TransactionId,@ClientName, @Status, @Type, @Amount);
            ";

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(mergeQuery, connection);
                command.Parameters.AddWithValue("@TransactionId", transaction.TransactionId);
                command.Parameters.AddWithValue("@Status", transaction.Status.ToString());
                command.Parameters.AddWithValue("@Type", transaction.Type.ToString());
                command.Parameters.AddWithValue("@ClientName", transaction.ClientName);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Transaction> UpdateTransactionStatus(Transaction transaction, Status? newStatus)
        {
            transaction.Status = (Status)newStatus;

            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();

            return transaction;
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
                Type = (Type)Enum.Parse(typeof(Type), await dataReader.GetFieldValueAsync<string>(typeIndex)),
                Amount = await dataReader.GetFieldValueAsync<decimal>(amountIndex)
            };

            return transaction;
        }
    }
}