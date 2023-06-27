using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PBH_API.Helper;
using PBH_API.Models;
using System.Data;

namespace PBH_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationController : Controller
    {
        private readonly string _connectionString;

        public DonationController(string connectionString)
        {
            _connectionString = connectionString;
        }
        [HttpPost("insertDonation")]
        public async Task<IActionResult> CreateDonation(DonationIn donation)
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
                                var userId = reader.GetInt32(reader.GetOrdinal("User_Id"));

                                if (userId == 0)
                                {
                                    return Unauthorized("User not found");
                                }
                                using (SqlConnection secondConnection = new SqlConnection(_connectionString)) {
                                    await secondConnection.OpenAsync();
                                    using (SqlCommand insertDonationCommand = new SqlCommand("dbo.insertDonation", secondConnection))
                                {
                                    insertDonationCommand.CommandType = CommandType.StoredProcedure;
                                    insertDonationCommand.Parameters.AddWithValue("@User_Id", userId);
                                    insertDonationCommand.Parameters.AddWithValue("@Book_Id", donation.BookId);
                                    insertDonationCommand.Parameters.AddWithValue("@DonationAmount", donation.DonationAmount);

                                    await insertDonationCommand.ExecuteNonQueryAsync();

                                    return Ok("Donation inserted successfully");
                                }
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

        [HttpGet("sumByBookId")]
        public async Task<IActionResult> GetAllDonationsSumByBook(int bookId)
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
                        await command.ExecuteNonQueryAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int userId = reader.GetInt32(reader.GetOrdinal("User_Id"));

                                if (userId == 0)
                                {
                                    return Unauthorized("User not found");
                                }

                                // Proceed with retrieving the sum
                                using (SqlCommand donationCommand = new SqlCommand("dbo.getAllDonationsSumByBook", connection))
                                {
                                    donationCommand.CommandType = CommandType.StoredProcedure;
                                    donationCommand.Parameters.AddWithValue("@Book_Id", bookId);

                                    using (var donationReader = await donationCommand.ExecuteReaderAsync())
                                    {
                                        if (await donationReader.ReadAsync())
                                        {
                                            decimal sum = donationReader.GetDecimal(donationReader.GetOrdinal("Quant"));

                                            var responseObj = new
                                            {
                                                HeaderValue = headerValue,
                                                UserId = userId,
                                                BookId = bookId,
                                                TotalDonations = sum
                                            };

                                            return Ok(responseObj);
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

        [HttpGet("sumByUserId")]
        public async Task<IActionResult> GetAllDonationsSumByUser()
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
                        await command.ExecuteNonQueryAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int validatedUserId = reader.GetInt32(reader.GetOrdinal("User_Id"));

                                if (validatedUserId == 0)
                                {
                                    return Unauthorized("User not found");
                                }

                                // Proceed with retrieving the sum
                                using (SqlConnection updateConnection = new SqlConnection(_connectionString))
                                {
                                    await updateConnection.OpenAsync();
                                    using (SqlCommand donationCommand = new SqlCommand("dbo.getAllDonationsSumByUser", updateConnection))
                                    {
                                        donationCommand.CommandType = CommandType.StoredProcedure;
                                        donationCommand.Parameters.AddWithValue("@User_Id", validatedUserId);

                                        using (var donationReader = await donationCommand.ExecuteReaderAsync())
                                        {
                                            if (await donationReader.ReadAsync())
                                            {
                                                if (!donationReader.IsDBNull(donationReader.GetOrdinal("Quant")))
                                                {
                                                    decimal sum = donationReader.GetDecimal(donationReader.GetOrdinal("Quant"));

                                                    var responseObj = new
                                                    {
                                                        HeaderValue = headerValue,
                                                        UserId = validatedUserId,
                                                        TotalDonations = sum
                                                    };

                                                    return Ok(responseObj);
                                                }
                                                else
                                                {
                                                    decimal sum = 0.0m;

                                                    var responseObj = new
                                                    {
                                                        HeaderValue = headerValue,
                                                        UserId = validatedUserId,
                                                        TotalDonations = sum
                                                    };

                                                    return Ok(responseObj);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return Unauthorized("User not found");
                    }
                }
            }
            else
            {
                return NotFound("Header not provided");
            }
        }

        [HttpGet("sumByWritter")]
        public async Task<IActionResult> GetAllDonationsSumByWriter()
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
                        await command.ExecuteNonQueryAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int validatedUserId = reader.GetInt32(reader.GetOrdinal("User_Id"));

                                if (validatedUserId == 0)
                                {
                                    return Unauthorized("User not found");
                                }

                                // Proceed with retrieving the sum
                                using (SqlConnection updateConnection = new SqlConnection(_connectionString))
                                {
                                    await updateConnection.OpenAsync();
                                    using (SqlCommand donationCommand = new SqlCommand("dbo.getAllDonationsSumByWritter", updateConnection))
                                    {
                                        donationCommand.CommandType = CommandType.StoredProcedure;
                                        donationCommand.Parameters.AddWithValue("@User_Id", validatedUserId);

                                        using (var donationReader = await donationCommand.ExecuteReaderAsync())
                                        {
                                            if (await donationReader.ReadAsync())
                                            {
                                                if (!donationReader.IsDBNull(donationReader.GetOrdinal("Quant")))
                                                {
                                                    decimal sum = donationReader.GetDecimal(donationReader.GetOrdinal("Quant"));

                                                    var responseObj = new
                                                    {
                                                        HeaderValue = headerValue,
                                                        UserId = validatedUserId,
                                                        TotalDonations = sum
                                                    };

                                                    return Ok(responseObj);
                                                }
                                                else
                                                {
                                                    decimal sum = 0.0m;

                                                    var responseObj = new
                                                    {
                                                        HeaderValue = headerValue,
                                                        UserId = validatedUserId,
                                                        TotalDonations = sum
                                                    };

                                                    return Ok(responseObj);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return Unauthorized("User not found");
                    }
                }
            }
            else
            {
                return NotFound("Header not provided");
            }
        }



    }
}