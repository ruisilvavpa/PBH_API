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

        [HttpGet("getAllBooks")]

        public async Task<IActionResult> GetAllBooks()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("getAllBooks", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Process the result set and return it as JSON
                        var books = new List<Book>();

                        while (await reader.ReadAsync())
                        {
                            // Map the columns to your book model properties
                            Book book = new Book();
                            {
                                book.Book_Id = reader.GetInt32(reader.GetOrdinal("Book_Id"));
                                book.Title = reader.GetString(reader.GetOrdinal("Title"));
                                book.Category = reader.GetInt32(reader.GetOrdinal("Category_Id"));
                                book.Description = reader.GetString(reader.GetOrdinal("Description"));
                                book.Media_Rating = reader.GetDecimal(reader.GetOrdinal("Media_Rating"));
                                book.Goal = reader.GetInt32(reader.GetOrdinal("Goal"));
                                book.User_Id = reader.GetInt32(reader.GetOrdinal("User_Id"));
                                book.Institution_Id = reader.GetInt32(reader.GetOrdinal("Institution_Id"));
                            };

                            books.Add(book);
                        }

                        return Ok(books);
                    }
                }
            }
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

        [HttpGet("getAllBooksByWritter")]

        public async Task<IActionResult> GetBooksByWritter(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("getAllBooksByWritter", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@User_Id", userId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Process the result set and return it as JSON
                        if (reader.HasRows)
                        {
                            var books = new List<Book>();

                            while (await reader.ReadAsync())
                            {
                                // Map the columns to your book model properties
                                Book book = new Book();
                                {
                                    book.Book_Id = reader.GetInt32(reader.GetOrdinal("Book_Id"));
                                    book.Title = reader.GetString(reader.GetOrdinal("Title"));
                                    book.Category = reader.GetInt32(reader.GetOrdinal("Category_Id"));
                                    book.Description = reader.GetString(reader.GetOrdinal("Description"));
                                    book.Media_Rating = reader.GetDecimal(reader.GetOrdinal("Media_Rating"));
                                    book.Goal = reader.GetInt32(reader.GetOrdinal("Goal"));
                                    book.User_Id = reader.GetInt32(reader.GetOrdinal("User_Id"));
                                    book.Institution_Id = reader.GetInt32(reader.GetOrdinal("Institution_Id"));
                                };

                                books.Add(book);
                            }

                            return Ok(books);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
        }
    }
}