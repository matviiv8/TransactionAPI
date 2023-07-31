using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using TransactionAPI.Domain.Models;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Accounts;

namespace TransactionAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IConfiguration configuration, IPasswordHasher passwordHasher)
        {
            this._configuration = configuration;
            this._passwordHasher = passwordHasher;
        }

        public async Task<User> Authenticate(LoginViewModel loginModel)
        {
            var user = await GetUserByUsername(loginModel.Username);

            return _passwordHasher.VerifyPassword(loginModel.Password, user.Password) ? user : throw new ArgumentException("Incorrect password.");
        }

        public async Task<User> GetUserByUsername(string username)
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

            return null;
        }

        public async Task<User> Register(User newUser)
        {
            newUser.Password = _passwordHasher.HashPassword(newUser.Password);

            try
            {
                await AddUserToDatabase(newUser);
                return newUser;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Failed to add user.", exception);
            }
        }

        private async Task AddUserToDatabase(User newUser)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"INSERT INTO Users (Username, Email, Password) 
                             VALUES (@username, @email, @password)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", newUser.Username);
                command.Parameters.AddWithValue("@email", newUser.Email);
                command.Parameters.AddWithValue("@password", newUser.Password);

                await connection.OpenAsync();

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected <= 0)
                {
                    throw new InvalidOperationException("Failed to add user.");
                }
            }
        }

        private async Task<User> ReadUser(SqlDataReader dataReader)
        {
            int usernameIndex = dataReader.GetOrdinal("Username");
            int passwordIndex = dataReader.GetOrdinal("Password");
            int emailIndex = dataReader.GetOrdinal("Email");

            User user = new User
            {
                Username = await dataReader.GetFieldValueAsync<string>(usernameIndex),
                Password = await dataReader.GetFieldValueAsync<string>(passwordIndex),
                Email = await dataReader.GetFieldValueAsync<string>(emailIndex),
            };

            return user;
        }
    }
}
