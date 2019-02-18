using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RobiGroup.Web.Common.Configuration;

namespace RobiGroup.Web.Common.Identity
{
    public static class IdentityHelpers
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static string GetUserId(this ClaimsIdentity user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return idClaim?.Value;
        }

        public static IdentityBuilder AddPhoneNumber4DigitTokenProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var phoneNumberProviderType = typeof(PhoneNumber4DigitTokenProvider<>).MakeGenericType(userType);
            return builder.AddTokenProvider(TokenOptions.DefaultPhoneProvider, phoneNumberProviderType);
        }
        /*
        public static bool HasPassword(this ClaimsPrincipal user)
        {
            return bool.Parse(user.FindFirstValue(UTutorClaimTypes.HasPassword));
        }

        public static bool IsPhoneConfirmed(this ClaimsPrincipal user)
        {
            return bool.Parse(user.FindFirstValue(UTutorClaimTypes.PhoneConfirmed));
        }

        public static string GetUserProfilePhoto(this HttpContext httpContext)
        {
            return GetUserProfilePhoto(httpContext, 0);
        }

        public static string GetUserProfilePhoto(this HttpContext httpContext, int thumnailSize)
        {
            return GetUserProfilePhoto(httpContext, null, thumnailSize);
        }

        public static string GetUserProfilePhoto(this HttpContext httpContext, string userId, int thumnailSize = 0)
        {
            string profilePhoto;

            var defaults = httpContext.RequestServices.GetService<IOptions<DefaultsOptions>>();

            if (string.IsNullOrEmpty(userId))
            {
                profilePhoto = httpContext.User.FindFirstValue(UTutorClaimTypes.ProfilePhoto);
            }
            else
            {
                var userManager = httpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
                var user = userManager.FindByIdAsync(userId);
                profilePhoto = user.Result == null ? defaults.Value.UserProfilePhoto : user.Result.ProfilePhoto;
            }

            if (string.IsNullOrEmpty(profilePhoto))
            {
                return defaults.Value.UserProfilePhoto;
            }

            var fileService = httpContext.RequestServices.GetService<IFileService>();
            if (thumnailSize > 0)
            {
                return fileService.Thumbnail(profilePhoto, thumnailSize);
            }

            return fileService.FilePathToURL(profilePhoto); 
        }*/
    }
}