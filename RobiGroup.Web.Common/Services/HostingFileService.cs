using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RobiGroup.Web.Common.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;

namespace RobiGroup.Web.Common.Services
{
    public class HostingFileService : IFileService
    {
        private const int DEFAULT_THUMBNAIL_SIZE = 144;

        private IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<DefaultsOptions> _defaults;

        public HostingFileService(IHostingEnvironment hostingEnvironment, IOptions<DefaultsOptions> defaults)
        {
            _hostingEnvironment = hostingEnvironment;
            _defaults = defaults;
        }

        
        public string Thumbnail(string path, int size = DEFAULT_THUMBNAIL_SIZE)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetWithWebRootPath(_defaults.Value.NotFoundImagePath);
            }

            if (!path.StartsWith(_hostingEnvironment.WebRootPath))
            {
                path = GetWithWebRootPath(path);
            }

            if (!File.Exists(path))
            {
                path = GetWithWebRootPath(_defaults.Value.NotFoundImagePath);
            }

            string thumbPath;

            if (FormatHelpers.IsImageFile(path))
            {
                thumbPath = GetThumbnailFileName(path, size);

                if (File.Exists(thumbPath))
                {
                    return FilePathToURL(thumbPath);
                }

                if (!File.Exists(thumbPath))
                {
                    thumbPath = SaveThumbnail(path, size);
                }
            }
            else
            {
                thumbPath = "";
            }

            return FilePathToURL(thumbPath);
        }

        private string GetWithWebRootPath(string path)
        {
            if (path.StartsWith(@"\"))
            {
                path = path.Remove(0, 1);
            } else if (path.StartsWith("//"))
            {
                path = path.Remove(0, 2);
            } else if (path.StartsWith("/"))
            {
                path = path.Remove(0, 1);
            }

            return Path.Combine(_hostingEnvironment.WebRootPath, path);
        }

        private string SaveThumbnail(string srcPath, int size)
        {
            if (string.IsNullOrEmpty(srcPath) || !File.Exists(srcPath))
            {
                return string.Empty;
            }

            var thumbPath = GetThumbnailFileName(srcPath, size);

            try
            {
                using (FileStream stream = File.OpenRead(srcPath))
                using (Image<Rgba32> image = Image.Load<Rgba32>(stream))
                {
                    float d = (float)image.Width / size;
                    image.Mutate(x => x.Resize((int)(image.Width / d), (int)(image.Height / d)));
                    image.Save(thumbPath);
                }

                return thumbPath;
            }
            catch (Exception e)
            {
                return srcPath;
            }
        }

        public string FilePathToURL(string path)
        {
            if (!File.Exists(path))
            {
                return _defaults.Value.NotFoundImagePath;
            }

            return path.Replace(_hostingEnvironment.WebRootPath, @"").Replace('\\', '/');
        }

        public async Task<string> Save(IFormFile file, string path)
        {
            
            var fileName = file.FileName;
            var dir = Path.Combine(_hostingEnvironment.WebRootPath, path);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string filePath = Path.Combine(dir, fileName);

            if (File.Exists(filePath))
            {
                filePath = Path.Combine(dir, fileName);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                return filePath;
            }
        }

        private string GetThumbnailFileName(string srcPath, int size)
        {
            string path = Path.GetDirectoryName(srcPath);
            string fileName = Path.GetFileNameWithoutExtension(srcPath);
            string extension = Path.GetExtension(srcPath);
            return Path.Combine(path, $"{fileName}_thumb_{size}{extension}");
        }
    }
}