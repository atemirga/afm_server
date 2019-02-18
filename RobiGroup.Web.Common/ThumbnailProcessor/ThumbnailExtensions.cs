using Microsoft.AspNetCore.Builder;

namespace RobiGroup.Web.Common.ThumbnailProcessor
{
    public static class ThumbnailExtensions
    {
        public static IApplicationBuilder UseThumbnailProcessor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThumbnailProcessorMiddleware>();
        }
    }
}