using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RobiGroup.AskMeFootball.Data;

namespace RobiGroup.AskMeFootball.Core.Identity
{
    public class AmfClaimsPrincipalFactory :
        UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly ApplicationDbContext _context;

        public AmfClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext context) :
            base(userManager, roleManager, optionsAccessor)
        {
            _context = context;
        }

        public async override Task<ClaimsPrincipal>
            CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);

            var identity = ((ClaimsIdentity)principal.Identity);
            identity.AddClaims(new[]
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.GivenName, user.FullName),
                new Claim(ClaimTypes.Email, user.Email), 
                
            });

            //if (await UserManager.IsInRoleAsync(user, UTutorRoles.Teacher))
            //{
            //    var teacher = _context.Teachers.Find(user.Id);
            //    identity.AddClaim(new Claim(ToihanaClaimTypes.TeacherLocked, teacher.IsLocked.ToString()));
            //}

            return principal;
        }
    }
}