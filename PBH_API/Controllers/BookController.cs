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

        [HttpPost]
        public async Task<int> InsertBook(BooksIn book)
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

                        int loggedInUserId = (int)await command.ExecuteScalarAsync();

                        if (loggedInUserId == 0)
                        {
                            throw new Exception("Failed to retrieve the book ID.");
                        }

                        using (SqlConnection secondConnection = new SqlConnection(_connectionString))
                        {
                            await secondConnection.OpenAsync();

                            using (SqlCommand secondCommand = new SqlCommand("dbo.AddBook", secondConnection))
                            {
                                secondCommand.CommandType = CommandType.StoredProcedure;

                                secondCommand.Parameters.AddWithValue("@Title", book.Title);
                                secondCommand.Parameters.AddWithValue("@Category_Id", book.Category_Id);
                                secondCommand.Parameters.AddWithValue("@Description", book.Description);
                                secondCommand.Parameters.AddWithValue("@Media_Rating", book.Media_Rating);
                                secondCommand.Parameters.AddWithValue("@Goal", book.Goal);
                                secondCommand.Parameters.AddWithValue("@User_Id", loggedInUserId);
                                secondCommand.Parameters.AddWithValue("@Institution_Id", book.Institution_Id);

                                var result = await secondCommand.ExecuteScalarAsync();

                                if (int.TryParse(result.ToString(), out int bookId))
                                {
                                    return bookId;
                                }
                                else
                                {
                                    // Erro ao obter o ID do livro inserido
                                    throw new Exception("Failed to retrieve the book ID.");
                                }
                            }
                        }
                    }
                }
            }
            throw new Exception("Failed to retrieve the header.");

        }
    }
}
