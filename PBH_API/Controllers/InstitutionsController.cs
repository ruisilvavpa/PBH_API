using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PBH_API.Models;
using System.Data;

namespace PBH_API.Controllers
{
    [Route("api/[controller]")]
    public class InstitutionsController : ControllerBase
    {
        private readonly string _connectionString;

        public InstitutionsController(string connectionString)
        {
            _connectionString = connectionString;
        }

        [HttpGet("institutions")]
        public async Task<IActionResult> GetInstitutions()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("dbo.GetInstitutions", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        List<Institution> institutions = new List<Institution>();

                        while (reader.Read())
                        {
                            Institution institution = new Institution
                            {
                                Institution_Id = Convert.ToInt32(reader["Institution_Id"]),
                                Name = reader["Name"].ToString(),
                                Descript = reader["Descript"].ToString(),
                                Iban = reader["Iban"].ToString()
                            };

                            institutions.Add(institution);
                        }

                        return Ok(institutions);
                    }
                }
            }
        }

    }
}
