using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PBH_API.Models;
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
            List<BookCategories> categories = new List<BookCategories>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("dbo.GetCategoryBooks", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            BookCategories category = new BookCategories();
                            category.Caterogy_Id = reader.GetInt32(reader.GetOrdinal("Category_Id"));
                            category.Category_Name = reader.GetString(reader.GetOrdinal("CategoryName"));
                            categories.Add(category);
                        }
                    }
                }
            }

            return Ok(categories);
        }

    }
}
