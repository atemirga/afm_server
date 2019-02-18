using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using RobiGroup.AskMeFootball.Areas.Admin.Models.Photos;
using RobiGroup.AskMeFootball.Common.Files;

namespace RobiGroup.AskMeFootball.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PhotosController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;

        public PhotosController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Upload()
        {
            var tempFolderPath = _hostingEnvironment.GetPhotosTempFolder();
            var filesPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(filesPath);

            List<UploadedPhotoModel> filePaths = new List<UploadedPhotoModel>();

            foreach (var file in Request.Form.Files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;

                // Ensure the file name is correct
                fileName = fileName.Contains("\\")
                    ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)
                    : fileName.Trim('"');

                var fullFilePath = Path.Combine(filesPath, fileName);

                if (file.Length <= 0)
                {
                    continue;
                }

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    filePaths.Add(new UploadedPhotoModel
                    {
                        Name = fileName,
                        Path = Path.GetRelativePath(tempFolderPath, fullFilePath)
                    });
                }
            }
            return Ok(filePaths);
        }

        [HttpPost]
        public IActionResult Delete(string path, int? restaurant)
        {
            var filePath = restaurant.HasValue ? Path.Combine(_hostingEnvironment.WebRootPath, path) : Path.Combine(_hostingEnvironment.GetPhotosTempFolder(), path);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);

                var dir = Path.GetDirectoryName(filePath);
                if (Directory.GetFiles(dir).Length == 0)
                {
                    Directory.Delete(dir);
                }

                return Ok();
            }

            return BadRequest();
        }

    }
}