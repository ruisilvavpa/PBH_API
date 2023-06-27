using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PBH_API.Helper;
using PBH_API.Models;
using System.Data;

namespace PBH_API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class RatingController : Controller
	{
		private readonly string _connectionString;

		public RatingController(string connectionString)
		{
			_connectionString = connectionString;
		}

		[HttpPost("insertRating")]
		public async Task<IActionResult> CreateRating(RatingIn rating)
		{
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

						using (var reader = await getUserIdCommand.ExecuteReaderAsync())
						{
							if (await reader.ReadAsync())
							{
								var User_Id = reader.GetInt32(reader.GetOrdinal("User_Id"));

								if (User_Id == 0)
								{
									return Unauthorized("User not found");
								}

								using (SqlCommand insertRatingCommand = new SqlCommand("dbo.insertRating", connection))
								{
									insertRatingCommand.CommandType = CommandType.StoredProcedure;
									insertRatingCommand.Parameters.AddWithValue("@Rating", rating.rating);
									insertRatingCommand.Parameters.AddWithValue("@User_Id", User_Id);
									insertRatingCommand.Parameters.AddWithValue("@Book_Id", rating.Book_Id);
									insertRatingCommand.Parameters.AddWithValue("@Comment", rating.Comment);

									await insertRatingCommand.ExecuteNonQueryAsync();

									return Ok("Rating inserted successfully");
								}
							}
						}
					}
					return NotFound("Header not provided");
				}
			}
			else
			{
				return NotFound("Header not provided");
			}
		}
	}
}
