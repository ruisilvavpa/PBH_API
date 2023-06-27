using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace PBH_API.Helper
{
    public class ImageHelper
    {
        //Store client image 
        public string storeImage(IFormFile? clientImage, string clientId)
        {

            string imagePath = "";
            try
            {
                if (clientImage != null && clientImage.Length > 0)
                {
                    string basePath = Directory.GetCurrentDirectory();
                    string uploadsPath = Path.Combine(basePath, "uploads", "clientsImages");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }
                    imagePath = uploadsPath + clientId.Trim()
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
    }
}
