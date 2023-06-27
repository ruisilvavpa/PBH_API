using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PBH_API.Helper;
using PBH_API.Models;
using System.Data;
using System.Net;
using System.Reflection.PortableExecutable;

namespace PBH_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(string connectionString, IWebHostEnvironment webHostEnvironment)
        {
            _connectionString = connectionString;
            _webHostEnvironment = webHostEnvironment;
        }

        //GET api/user/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
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
                                var userID = reader.GetInt32(reader.GetOrdinal("User_Id"));
                                
                                if (userID == 0)
                                {
                                    return Unauthorized("User not found");
                                }
                                UserHelper userHelper = new UserHelper();
                                UsersOut? user = await userHelper.GetUserById(userID, connectionString: _connectionString);

                                var responseObj = new
                                {
                                    HeaderValue = headerValue,
                                    User = user
                                };

                                return Ok(responseObj);
                            }
                        }
                    }
                    return NotFound("Header não fornecido");

                }

            }
            else
            {
                return NotFound("Header não fornecido");
            }
        }

        //GET writtersByTypeAccount
        [HttpGet("writters")]
        public async Task<IActionResult> GetUsersByTypeAccount1()
        {
            List<UsersOut> users = new List<UsersOut>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("GetUserByTypeAccount1", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UsersOut user = new UsersOut();

                            user.Id = reader.GetInt32(reader.GetOrdinal("User_Id"));
                            user.Name = reader.GetString(reader.GetOrdinal("Name"));
                            user.Email = reader.GetString(reader.GetOrdinal("Email"));
                            user.Type = reader.GetInt32(reader.GetOrdinal("TypeAccount"));
                            int bioOrdinal = reader.GetOrdinal("Bio");
                            if (!reader.IsDBNull(bioOrdinal))
                            {
                                user.Bio = reader.GetString(bioOrdinal);
                            }

                            users.Add(user);
                        }
                    }
                    
                }
                
            }

            return Ok(users);
        }

        //GET userById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsersById(int userId)
        {
            UserHelper userHelper = new UserHelper();
            UsersOut? user = await userHelper.GetUserById(userId, connectionString: _connectionString);
            return Ok(user);
        }


        // PUT api/user
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UsersOut user)
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
                            return Unauthorized("User não encontrado");
                        }

                        if (loggedInUserId != user.Id)
                        {
                            return Unauthorized("Não é possível atualizar as informações de outro user");
                        }

                        using (SqlConnection updateConnection = new SqlConnection(_connectionString))
                        {
                            await updateConnection.OpenAsync();

                            using (SqlCommand updateCommand = new SqlCommand("dbo.UpdateUser", updateConnection))
                            {
                                updateCommand.CommandType = CommandType.StoredProcedure;

                                updateCommand.Parameters.AddWithValue("@UserId", user.Id);
                                updateCommand.Parameters.AddWithValue("@Name", user.Name);
                                updateCommand.Parameters.AddWithValue("@Email", user.Email);
                                updateCommand.Parameters.AddWithValue("@Bio", user.Bio);

                                await updateCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                return Ok();
            }
            else
            {
                return NotFound("Header não fornecido");
            }
        }



        //DELETE api/user
        [HttpDelete]
        public async Task<IActionResult> DeleteById()
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
                                var userID = reader.GetInt32(reader.GetOrdinal("User_Id"));

                                if (userID == 0)
                                {
                                    return Unauthorized("User not found");

                                }
                                using (SqlConnection secondConnection = new SqlConnection(_connectionString))
                                {
                                    await secondConnection.OpenAsync();
                                    using (SqlCommand secondcommand = new SqlCommand("dbo.DeleteUser", secondConnection))
                                    {
                                        secondcommand.CommandType = CommandType.StoredProcedure;
                                        secondcommand.Parameters.AddWithValue("@UserId", userID);

                                        await secondcommand.ExecuteNonQueryAsync();
                                    }
                                }
                                
                            }
                            
                        }
                    }
                }
                return Ok();
            }
            else
            {
                return NotFound("Header não fornecido");
            }
        }

        //PUT
        [HttpPut("changePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] ChangePassword password)
        {

            if (password.newPassword != password.confirmPassword)
            {
                return ValidationProblem();
            }

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
                            return Unauthorized("User não encontrado");
                        }
                       


                        using (SqlConnection updateConnection = new SqlConnection(_connectionString))
                        {
                            await updateConnection.OpenAsync();

                            using (SqlCommand updateCommand = new SqlCommand("dbo.ResetPassword", updateConnection))
                            {
                                updateCommand.CommandType = CommandType.StoredProcedure;

                                updateCommand.Parameters.AddWithValue("@Password", password.oldPassword);
                                updateCommand.Parameters.AddWithValue("@NewPassword", password.newPassword);
                                updateCommand.Parameters.AddWithValue("@UserId", loggedInUserId);

                                await updateCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                return Ok();
            }
            else
            {
                return NotFound("Header não fornecido");
            }
        }

        //PUT
        [HttpPut("userImage")]
        public async Task<IActionResult> UpdateUserImage([FromForm] Image form)
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

                        int loggedInUserId = (int) await command.ExecuteScalarAsync();

                        if (loggedInUserId == 0)
                        {
                            return Unauthorized("User não encontrado");
                        }
                        string imagePath = storeUserImage(form.ImageSent, loggedInUserId.ToString());


                        using (SqlConnection updateConnection = new SqlConnection(_connectionString))
                        {
                            await updateConnection.OpenAsync();

                            using (SqlCommand updateCommand = new SqlCommand("dbo.UpdateUserImage", updateConnection))
                            {
                                updateCommand.CommandType = CommandType.StoredProcedure;

                                updateCommand.Parameters.AddWithValue("@ImagePath", imagePath);
                                updateCommand.Parameters.AddWithValue("@UserId", loggedInUserId);

                                await updateCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                return Ok();
            }
            else
            {
                return NotFound("Header não fornecido");
            }
        }

        //Store client image 
        private string storeUserImage(IFormFile? clientImage, string clientName)
        {

            string imagePath = "";
            try
            {
                if (clientImage != null && clientImage.Length > 0)
                {
                    string path = _webHostEnvironment.WebRootPath + "\\uploads\\clientsImages\\";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    imagePath = path + clientName.Trim()
                        + "_" + DateTime.Now.ToString("yyyyMMddHHmmss")
                        + "_" + clientImage.FileName; ;

                    using (FileStream fileStream = System.IO.File.Create(imagePath))
                    {
                        clientImage.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return imagePath;
        }

		[HttpGet("getWritterByBook")]

		public async Task<IActionResult> GetWritterByBook(int bookId)
		{
			UsersOut user = new UsersOut();
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				using (SqlCommand command = new SqlCommand("getWritterByBook", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@Book_Id", bookId);

					using (SqlDataReader reader = await command.ExecuteReaderAsync())
					{
						
						while (await reader.ReadAsync())
							{
								// Map the columns to your book model properties
								

								user.Id = reader.GetInt32(reader.GetOrdinal("User_Id"));
								user.Name = reader.GetString(reader.GetOrdinal("Name"));
								user.Email = reader.GetString(reader.GetOrdinal("Email"));
								user.Type = reader.GetInt32(reader.GetOrdinal("TypeAccount"));
								int bioOrdinal = reader.GetOrdinal("Bio");
								if (!reader.IsDBNull(bioOrdinal))
								{
									user.Bio = reader.GetString(bioOrdinal);
								}
							}

				
						
					}
				}
                return Ok(user);
			}

		}


	}
}
