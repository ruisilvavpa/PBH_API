using Microsoft.AspNetCore.Mvc;

namespace PBH_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly string _connectionString;

        public ImageController(string connectionString)
        {
            _connectionString = connectionString;
        }

        [HttpGet("getImage")]
        public IActionResult GetImage([FromQuery] string imagePath)
        {
            // Check if the file path is provided
            if (string.IsNullOrEmpty(imagePath))
            {
                return BadRequest("Image path is required.");
            }

            // Check if the file exists
            if (System.IO.File.Exists(imagePath))
            {
                // Read the file content as bytes
                var fileContent = System.IO.File.ReadAllBytes(imagePath);

                // Convert the file content to base64
                var base64String = Convert.ToBase64String(fileContent);

                // Return the base64 string and imagePath as the response
                return Ok(new { imageBase64 = base64String, imagePath = imagePath });
            }

            // Return a not found response if the file doesn't exist
            return NotFound();
        }

    }
}
