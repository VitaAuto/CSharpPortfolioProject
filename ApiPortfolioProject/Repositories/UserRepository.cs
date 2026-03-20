using ApiPortfolioProject.Models;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ApiPortfolioProject.Repositories
{
    public class UserRepository(IConfiguration config) : IUserRepository
    {
        private readonly string? _connectionString = config.GetConnectionString("DefaultConnection");

        public User? GetUser(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection.QueryFirstOrDefault<User>("SELECT * FROM \"Users\" WHERE \"Id\" = @Id", new { Id = id });
        }

        public List<User> GetAllUsers()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection.Query<User>("SELECT * FROM \"Users\"").AsList();
        }
        public User? CreateUser(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            if (string.IsNullOrWhiteSpace(user.CreatedOn))

                user.CreatedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            if (string.IsNullOrWhiteSpace(user.ModifiedOn))
                user.ModifiedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var sql = @"INSERT INTO ""Users"" (""FirstName"", ""LastName"", ""Email"", ""IsActive"", ""CreatedOn"", ""ModifiedOn"")
                        VALUES (@FirstName, @LastName, @Email, @IsActive, @CreatedOn, @ModifiedOn)
                        RETURNING ""Id"";";
            var id = connection.ExecuteScalar<int>(sql, user);
            user.Id = id;
            return GetUser(id);
        }
        public User? UpdateUser(int id, User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            user.ModifiedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var sql = @"UPDATE ""Users""
                        SET ""FirstName"" = @FirstName,
                            ""LastName"" = @LastName,
                            ""Email"" = @Email,
                            ""IsActive"" = @IsActive,
                            ""ModifiedOn"" = @ModifiedOn
                        WHERE ""Id"" = @Id";
            connection.Execute(sql, new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.IsActive,
                user.ModifiedOn,
                Id = id
            });
            return GetUser(id);
        }
        public User? PatchUser(int id, UserPatchDto patch)
        {
            var user = GetUser(id);
            if (user == null) return null;

            if (patch.FirstName != null) user.FirstName = patch.FirstName;
            if (patch.LastName != null) user.LastName = patch.LastName;
            if (patch.Email != null) user.Email = patch.Email;
            if (patch.IsActive.HasValue) user.IsActive = patch.IsActive.Value;

            user.ModifiedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            return UpdateUser(id, user);
        }
        public bool DeleteUser(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var sql = "DELETE FROM \"Users\" WHERE \"Id\" = @Id";
            var affected = connection.Execute(sql, new { Id = id });
            return affected > 0;
        }
        public bool EmailExists(string email, int? excludeId = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var sql = excludeId.HasValue
                ? "SELECT COUNT(1) FROM \"Users\" WHERE \"Email\" = @Email AND \"Id\" != @Id"
                : "SELECT COUNT(1) FROM \"Users\" WHERE \"Email\" = @Email";
            var count = connection.ExecuteScalar<int>(sql, new { Email = email, Id = excludeId });
            return count > 0;
        }
    }
}