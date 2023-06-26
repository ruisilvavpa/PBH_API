using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PBH_API.Models;
using System.Data;

namespace PBH_API.Controllers
{

	[ApiController]
	[Route("api/[controller]")]
	public class BookController : Controller
	{
		private readonly string _connectionString;

		public BookController(string connectionString)
		{
			_connectionString = connectionString;
		}
		[HttpPost("insertBook")]
		public async Task<IActionResult> CreateBook(Book book)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (SqlCommand command = new SqlCommand("insertBook", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@Title", book.Title);
					command.Parameters.AddWithValue("@Category", book.Category);
					command.Parameters.AddWithValue("@Description", book.Description);
					command.Parameters.AddWithValue("@Media_Rating", book.Media_Rating);
					command.Parameters.AddWithValue("@Goal", book.Goal);
					command.Parameters.AddWithValue("@User_Id", book.User_Id);
					command.Parameters.AddWithValue("@Insitution_Id", book.Institution_Id);


					await command.ExecuteNonQueryAsync();

					// Return a success response
					return Ok(book);

				}
			}
		}



		[HttpGet("getAllBooks")]

		public async Task<IActionResult> GetAllBooks(int pageSize, int pageNumber)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				using (SqlCommand command = new SqlCommand("getAllBooks", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@PageSize", pageSize);
					command.Parameters.AddWithValue("@PageNumber", pageNumber);

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
									book.Category = reader.GetString(reader.GetOrdinal("Category"));
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
									book.Category = reader.GetString(reader.GetOrdinal("Category"));
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
