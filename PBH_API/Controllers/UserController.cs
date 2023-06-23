using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PBH_API.Helper;
using PBH_API.Models;
using System.Data;
using System.Reflection.PortableExecutable;

namespace PBH_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;

        public UserController(string connectionString)
        {
            _connectionString = connectionString;
        }
        //GET api/user/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var headers = Request.Headers;
            if (headers.TryGetValue("Token", out var headerValue))
            {
                var token = headerValue.ToString();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("dbo.GetUserIdByToken", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Token", token);
                        await command.ExecuteNonQueryAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var userID = reader.GetInt32(reader.GetOrdinal("User_Id"));
                                
                                if (userID == 0)
                                {
                                    return Unauthorized("User not found");
                                }
                                UserHelper userHelper = new UserHelper();
                                UsersOut? user = await userHelper.GetUserById(userID, connectionString: _connectionString);

                                var responseObj = new
                                {
                                    HeaderValue = headerValue,
                                    User = user
                                };

                                return Ok(responseObj);
                            }
                        }
                    }
                    return NotFound("Header não fornecido");

                }

            }
            else
            {
                return NotFound("Header não fornecido");
            }
        }

        //GET writtersByTypeAccount
        [HttpGet("writters")]
        public async Task<IActionResult> GetUsersByTypeAccount1()
        {
            List<UsersOut> users = new List<UsersOut>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("GetUserByTypeAccount1", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UsersOut user = new UsersOut();

                            user.Id = reader.GetInt32(reader.GetOrdinal("User_Id"));
                            user.Name = reader.GetString(reader.GetOrdinal("Name"));
                            user.Email = reader.GetString(reader.GetOrdinal("Email"));
                            user.Type = reader.GetInt32(reader.GetOrdinal("TypeAccount"));
                            int bioOrdinal = reader.GetOrdinal("Bio");
                            if (!reader.IsDBNull(bioOrdinal))
                            {
                                user.Bio = reader.GetString(bioOrdinal);
                            }

                            users.Add(user);
                        }
                    }
                    
                }
                
            }

            return Ok(users);
        }

        //GET userById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsersById(int userId)
        {
            UserHelper userHelper = new UserHelper();
            UsersOut? user = await userHelper.GetUserById(userId, connectionString: _connectionString);
            return Ok(user);
        }

    }
}
