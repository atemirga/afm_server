using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace RobiGroup.AskMeFootball.Common.Files
{
    public static class PhotosPathHelpers
    {
        public static string GetPhotosTempFolder(this IHostingEnvironment hostingEnvironment)
        {
            return $"{hostingEnvironment.WebRootPath}/files/temp";
        }

        public static string GetCardPhotosFolder(this IHostingEnvironment hostingEnvironment, int cardId)
        {
            return $"{hostingEnvironment.WebRootPath}/files/cards/{cardId}";
        }

        public static List<string> GetRestaurantPhotos(this IHostingEnvironment hostingEnvironment, int cardId)
        {
            var photosFolder = hostingEnvironment.GetCardPhotosFolder(cardId);

            if (!Directory.Exists(photosFolder))
            {
                return new List<string>();
            }

            return Directory.GetFiles(photosFolder).Select(r => Path.GetRelativePath(hostingEnvironment.WebRootPath, r).Replace('\\', '/')).ToList();
        }
    }
}