using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PBH_API.Helper;
using PBH_API.Models;
using System.Data;

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
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("dbo.GetUserIdByToken", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Token", headerValue);

                        var insertedIdParameter = new SqlParameter("@InsertedId", SqlDbType.Int);
                        insertedIdParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(insertedIdParameter);

                        await command.ExecuteNonQueryAsync();

                        int? insertedId = insertedIdParameter.Value as int?;

                        if (insertedId == null)
                        {
                            return Unauthorized("User not found");
                        }
                        UserHelper userHelper = new UserHelper();
                        UsersOut? user = await userHelper.GetUserById(insertedId ?? 0, _connectionString);

                        return Ok(user);
                    }
                }

            }
            else
            {
                // O header não foi encontrado
                // Retorne uma resposta de erro
                return NotFound("Header não fornecido");
            }
        }
    }
}
