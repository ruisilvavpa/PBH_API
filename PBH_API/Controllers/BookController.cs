using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PBH_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly string _connectionString;

        public BookController(string connectionString)
        {
            _connectionString = connectionString;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetBookCategories()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("dbo.GetCategoryBooks", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        List<string> categories = new List<string>();

                        while (reader.Read())
                        {
                            string categoryName = reader["CategoryName"].ToString();
                            categories.Add(categoryName);
                        }

                        return Ok(categories);
                    }
                }
            }
        }
    }
}
