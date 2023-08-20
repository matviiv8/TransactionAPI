using TransactionAPI.Infrastructure.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using TransactionAPI.Domain.Models;
using Type = TransactionAPI.Domain.Enums.Type;
using TransactionAPI.Infrastructure.Interfaces.Transactions;
using TransactionAPI.Domain.Enums;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TransactionAPI.Application.Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly TransactionAPIDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(TransactionAPIDbContext dbContext, IConfiguration configuration, ILogger<TransactionService> logger)
        {
            this._dbContext = dbContext;
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task<List<Transaction>> GetTransactionsByFilter(List<Domain.Enums.Type>? types, Status? status, string? clientName = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    var queryBuilder = new StringBuilder("SELECT * FROM Transactions WHERE 1=1");

                    queryBuilder.Append(types != null && types.Any() ? " AND Type IN (" + string.Join(",", types.Select(t => $"'{t.ToString().Replace("'", "''")}'")) + ")" : "");
                    queryBuilder.Append(status != null ? " AND Status = @status" : "");
                    queryBuilder.Append(!string.IsNullOrEmpty(clientName) ? " AND ClientName LIKE @clientName" : "");

                    var query = queryBuilder.ToString();

                    var command = new SqlCommand(query, connection);

                    if (status != null)
                    {
                        command.Parameters.AddWithValue("@status", status.ToString());
                    }

                    if (!string.IsNullOrEmpty(clientName))
                    {
                        command.Parameters.AddWithValue("@clientName", "%" + clientName + "%");
                    }

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
            catch (Exception exception)
            {
                _logger.LogError($"Error in TransactionService.GetTransactionsByFilter(types, status, clientName = null): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while retrieving transactions by filter.", exception);
            }
        }

        public async Task<Transaction> GetTransactionById(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    string query = @"SELECT * 
                             FROM Transactions
                             WHERE Transactions.TransactionId = @id";

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
            catch (Exception exception)
            {
                _logger.LogError($"Error in TransactionService.GetTransactionById(id): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while getting transaction by ID.", exception);
            }
        }

        public async Task MergeTransaction(Transaction transaction)
        {
            try
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
                        VALUES (@TransactionId,@ClientName, @Status, @Type, @Amount);";

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
            catch (Exception exception)
            {
                _logger.LogError($"Error in TransactionService.MergeTransaction(transaction): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while merging transaction.", exception);
            }
        }

        public async Task<Transaction> UpdateTransactionStatus(Transaction transaction, Status? newStatus)
        {
            try
            {
                transaction.Status = (Status)newStatus;

                _dbContext.Transactions.Update(transaction);
                await _dbContext.SaveChangesAsync();

                return transaction;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in TransactionService.UpdateTransactionStatus(transaction, newStatus): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while updating transaction status.", exception);
            }
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