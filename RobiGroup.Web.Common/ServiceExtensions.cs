using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace RobiGroup.Web.Common
{
    public static class ServiceExtensions
    {
        public static void AddRolesToDbIfNotExists<TRole>(this RoleManager<TRole> roleManager, string[] roles) where TRole : IdentityRole, new()
        {
            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    roleManager.CreateAsync(new TRole()
                    {
                        Name = role
                    }).Wait();
                }
            }
        }
    }
}