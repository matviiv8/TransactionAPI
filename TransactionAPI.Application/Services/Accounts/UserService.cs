using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces.Accounts;

namespace TransactionAPI.Application.Services.Accounts
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(IConfiguration configuration, ILogger<UserService> logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    string query = @"SELECT * FROM Users WHERE Username = @username";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);

                    await connection.OpenAsync();

                    using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        if (await dataReader.ReadAsync())
                        {
                            return await ReadUser(dataReader);
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                _logger.LogError($"Error in UserService.GetUserByUsername(username): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while adding user to the database.", exception);
            }

            return null;
        }

        public async Task AddUserToDatabase(User newUser)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    string query = @"INSERT INTO Users (Username, Email, Password, RefreshToken) 
                             VALUES (@username, @email, @password, @refreshToken)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", newUser.Username);
                    command.Parameters.AddWithValue("@email", newUser.Email);
                    command.Parameters.AddWithValue("@password", newUser.Password);
                    command.Parameters.AddWithValue("@refreshToken", newUser.RefreshToken);

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected <= 0)
                    {
                        throw new InvalidOperationException("Failed to add user.");
                    }
                }
            }
            catch(Exception exception)
            {
                _logger.LogError($"Error in UserService.AddUserToDatabase(newUser): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while adding user to the database.", exception);
            }
        }

        public async Task<User> GetUserByRefreshToken(string refreshToken)
        {
            try 
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    string query = @"SELECT * FROM Users WHERE RefreshToken = @refreshToken";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@refreshToken", refreshToken);

                    await connection.OpenAsync();

                    using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        if (await dataReader.ReadAsync())
                        {
                            return await ReadUser(dataReader);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in UserService.GetUserByRefreshToken(refreshToken): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while fetching user by refresh token from the database.", exception);
            }

            return null;
        }

        private async Task<User> ReadUser(SqlDataReader dataReader)
        {
            int usernameIndex = dataReader.GetOrdinal("Username");
            int passwordIndex = dataReader.GetOrdinal("Password");
            int emailIndex = dataReader.GetOrdinal("Email");
            int refreshTokenIndex = dataReader.GetOrdinal("RefreshToken");

            User user = new User
            {
                Username = await dataReader.GetFieldValueAsync<string>(usernameIndex),
                Password = await dataReader.GetFieldValueAsync<string>(passwordIndex),
                Email = await dataReader.GetFieldValueAsync<string>(emailIndex),
                RefreshToken = await dataReader.GetFieldValueAsync<string>(refreshTokenIndex),
            };

            return user;
        }

        public async Task UpdateRefreshToken(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    string query = @"UPDATE Users SET RefreshToken = @refreshToken WHERE Username = @username";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@refreshToken", user.RefreshToken);
                    command.Parameters.AddWithValue("@username", user.Username);

                    await connection.OpenAsync();

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected <= 0)
                    {
                        throw new InvalidOperationException("Failed to update user's refresh token.");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in UserService.UpdateRefreshToken(user): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");

                throw new ApplicationException("Error while updating user's refresh token in the database.", exception);
            }
        }
    }
}
