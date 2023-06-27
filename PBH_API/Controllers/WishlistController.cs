using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using PBH_API.Models;
using System.Data;
using System.Diagnostics.Eventing.Reader;

namespace PBH_API.Controllers
{

	[ApiController]
	[Route("api/[controller]")]
	public class WishlistController : Controller
	{
		private readonly string _connectionString;

		public WishlistController(string connectionString)
		{
			_connectionString = connectionString;
		}

		[HttpPost("insertWishlist")]
		public async Task<IActionResult> CreateWishlist(WishlistIn wl)
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

								using (SqlCommand insertWishlistCommand = new SqlCommand("dbo.insertWishlist", connection))
								{
									insertWishlistCommand.CommandType = CommandType.StoredProcedure;
									insertWishlistCommand.Parameters.AddWithValue("@Book_Id", wl.Book_Id);
									insertWishlistCommand.Parameters.AddWithValue("@User_Id", User_Id);
									insertWishlistCommand.Parameters.AddWithValue("@Obs", wl.Obs);

									await insertWishlistCommand.ExecuteNonQueryAsync();

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
		[HttpDelete]
		public async Task<IActionResult> DeleteWishlist(int Wishlist_Id)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (SqlCommand deleteWishlistCommand = new SqlCommand("dbo.deleteWishlist", connection))
				{
					deleteWishlistCommand.CommandType = CommandType.StoredProcedure;
					deleteWishlistCommand.Parameters.AddWithValue("@Wishlist_Id", Wishlist_Id);

					await deleteWishlistCommand.ExecuteNonQueryAsync();

				}
				return Ok();
			}
		}



	}
}
