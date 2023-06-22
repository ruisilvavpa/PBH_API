using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.SqlClient;
using PBH_API.Models;
using System.Data;

namespace PBH_API.Helper
{
    public class UserHelper
    {
        public async Task<UsersOut?> GetUserByEmail(string email, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("dbo.GetUserByEmail", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            UsersOut user = new UsersOut();
                            {
                                user.Name = reader.GetString(reader.GetOrdinal("Name"));
                                user.Email = reader.GetString(reader.GetOrdinal("Email"));
                                user.Type = reader.GetInt32(reader.GetOrdinal("TypeAccount"));
                                user.Bio = reader.GetString(reader.GetOrdinal("Bio"));
                            }
                            return user;
                        }
                    }
                    return null;
                }
            }
        }

        public async Task<UsersOut?> GetUserById(int id, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("dbo.GetUserById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@User_Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            UsersOut user = new UsersOut();
                            {
                                user.Name = reader.GetString(reader.GetOrdinal("Name"));
                                user.Email = reader.GetString(reader.GetOrdinal("Email"));
                                user.Type = reader.GetInt32(reader.GetOrdinal("TypeAccount"));
                                user.Bio = reader.GetString(reader.GetOrdinal("Bio"));
                            }
                            return user;
                        }
                    }
                    return null;
                }
            }
        }
    }
}