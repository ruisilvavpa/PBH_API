using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PBH_API.Helper;
using PBH_API.Models;
using System.Data;

namespace PBH_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly string _connectionString;

        public AuthenticationController(string connectionString)
        {
            _connectionString = connectionString;
        }
        //POST authentication/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UsersIn user)
        {
            if (user == null)
            {
                return BadRequest("Invalid request body");
            }
            else
            {
                UserHelper userHelper = new UserHelper();
                UsersOut? checkUser = await userHelper.GetUserByEmail(user.Email, _connectionString);
                if (checkUser != null)
                {
                    return Conflict("User already exists");
                }
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("dbo.Register", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Name", user.Name);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@Password", user.Password);
                        command.Parameters.AddWithValue("@User_Type", user.Type);

                        var insertedIdParameter = new SqlParameter("@InsertedId", SqlDbType.Int);
                        insertedIdParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(insertedIdParameter);

                        await command.ExecuteNonQueryAsync();

                        int insertedId = (int)insertedIdParameter.Value;

                        var tokenHelper = new TokenHelper();
                        
                        var token = tokenHelper.GenerateToken();

                        using (SqlConnection secondConnection = new SqlConnection(_connectionString))
                        {
                            await secondConnection.OpenAsync();

                            using (SqlCommand secondCommand = new SqlCommand("dbo.AddToken", secondConnection))
                            {
                                secondCommand.CommandType = CommandType.StoredProcedure;

                                secondCommand.Parameters.AddWithValue("@Token", token);
                                secondCommand.Parameters.AddWithValue("@User_Id", insertedId);

                                await secondCommand.ExecuteNonQueryAsync();
                            }

                        }

                        SessionToken sessionToken = new SessionToken();
                        sessionToken.Token = token;
                        sessionToken.UserId = insertedId;
                        return Ok(sessionToken);                        
                    }


                }
            }
        }

        //POST authentication/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(Login login)
        {
            if (login == null)
            {
                return BadRequest("Invalid request body");
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("dbo.Login", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Email", login.Email);
                        command.Parameters.AddWithValue("@Password", login.Password);
                        

                        var insertedIdParameter = new SqlParameter("@InsertedId", SqlDbType.Int);
                        insertedIdParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(insertedIdParameter);

                        await command.ExecuteNonQueryAsync();

                        int? insertedId = insertedIdParameter.Value as int?;

                        if (insertedId == null)
                        {
                            return Unauthorized("User not found");
                        }

                        var tokenHelper = new TokenHelper();

                        var token = tokenHelper.GenerateToken();

                        using (SqlConnection secondConnection = new SqlConnection(_connectionString))
                        {
                            await secondConnection.OpenAsync();

                            using (SqlCommand secondCommand = new SqlCommand("dbo.AddToken", secondConnection))
                            {
                                secondCommand.CommandType = CommandType.StoredProcedure;

                                secondCommand.Parameters.AddWithValue("@Token", token);
                                secondCommand.Parameters.AddWithValue("@User_Id", insertedId);

                                await secondCommand.ExecuteNonQueryAsync();
                            }

                        }
                        SessionToken sessionToken = new SessionToken();
                        sessionToken.Token = token;
                        sessionToken.UserId = insertedId ?? 0;
                        return Ok(sessionToken);
                    }
                }
            }
        }

        //GET api/authentication/logout
        [HttpGet("logout")]
        public async Task<IActionResult> logout()
        {
            var headers = Request.Headers;
            if (headers.TryGetValue("Token", out var headerValue))
            {
                var token = headerValue.ToString();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("dbo.Logout", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Token", token);
                        await command.ExecuteNonQueryAsync();
                    }
                    return Ok();
                }

            }
            else
            {
                return NotFound("Header não fornecido");
            }
        }
    }
}
