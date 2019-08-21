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

        public static string GetCardTeamLogosFolder(this IHostingEnvironment hostingEnvironment, int cardId)
        {
            return $"{hostingEnvironment.WebRootPath}/files/cards/{cardId}/logos";
        }

        public static string GetQuestionBoxFolder(this IHostingEnvironment hostingEnvironment, int questionId)
        {
            return $"{hostingEnvironment.WebRootPath}/files/box/{questionId}";
        }

        public static string GetInfoCardPhotosFolder(this IHostingEnvironment hostingEnvironment, int cardId)
        {
            return $"{hostingEnvironment.WebRootPath}/files/infocards/photos/{cardId}";
        }

        public static string GetInfoCardImagesFolder(this IHostingEnvironment hostingEnvironment, int cardId)
        {
            return $"{hostingEnvironment.WebRootPath}/files/infocards/images/{cardId}";
        }

        public static string GetInfoCardVideosFolder(this IHostingEnvironment hostingEnvironment, int cardId)
        {
            return $"{hostingEnvironment.WebRootPath}/files/infocards/videos/{cardId}";
        }

        public static string GetPrizePhotosFolder(this IHostingEnvironment hostingEnvironment, int prizeId)
        {
            return $"{hostingEnvironment.WebRootPath}/files/prizes/{prizeId}";
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