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

			var headers = Request.Headers;
			if (headers.TryGetValue("Token", out var headerValue))
			{
				var token = headerValue.ToString();

				using (SqlConnection connection = new SqlConnection(_connectionString))
				{
					await connection.OpenAsync();

					using (SqlCommand getUserIdCommand = new SqlCommand("dbo.GetUserIdByToken", connection))
					{
						getUserIdCommand.CommandType = CommandType.StoredProcedure;
						getUserIdCommand.Parameters.AddWithValue("@Token", token);
						await getUserIdCommand.ExecuteNonQueryAsync();

						using (var reader = await getUserIdCommand.ExecuteReaderAsync())
						{
							if (await reader.ReadAsync())
							{
								int userId = reader.GetInt32(reader.GetOrdinal("User_Id"));

								if (userId == 0)
								{
									return Unauthorized("User not found");
								}

								// Proceed with inserting the book
								using (SqlCommand insertBookCommand = new SqlCommand("insertBook", connection))
								{
									insertBookCommand.CommandType = CommandType.StoredProcedure;
									insertBookCommand.Parameters.AddWithValue("@Title", book.Title);
									insertBookCommand.Parameters.AddWithValue("@Category", book.Category);
									insertBookCommand.Parameters.AddWithValue("@Description", book.Description);
									insertBookCommand.Parameters.AddWithValue("@Media_Rating", book.Media_Rating);
									insertBookCommand.Parameters.AddWithValue("@Goal", book.Goal);
									insertBookCommand.Parameters.AddWithValue("@User_Id", userId);
									insertBookCommand.Parameters.AddWithValue("@Insitution_Id", book.Institution_Id);

									await insertBookCommand.ExecuteNonQueryAsync();

									// Retrieve the sum of donations by writer
									using (SqlCommand getDonationsSumCommand = new SqlCommand("dbo.getAllDonationsSumByWritter", connection))
									{
										getDonationsSumCommand.CommandType = CommandType.StoredProcedure;
										getDonationsSumCommand.Parameters.AddWithValue("@User_Id", userId);

										using (var donationReader = await getDonationsSumCommand.ExecuteReaderAsync())
										{
											if (await donationReader.ReadAsync())
											{
												decimal sum = donationReader.GetDecimal(donationReader.GetOrdinal("Quant"));

												var responseObj = new
												{
													HeaderValue = headerValue,
													UserId = userId,
													TotalDonations = sum,
													Book = book
												};

												return Ok(responseObj);
											}
										}
									}
								}
							}
						}
					}
				}

				return Unauthorized("User not found");
			}
			else
			{
				return NotFound("Header not provided");
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

