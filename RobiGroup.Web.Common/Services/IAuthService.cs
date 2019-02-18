using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RobiGroup.Web.Common.Configuration;
using RobiGroup.Web.Common.Identity;
using RobiGroup.Web.Common.Services.Models;
using RobiGroup.Web.Common.Validation;

namespace RobiGroup.Web.Common.Services
{
    public interface IAuthService<TUser, TTokenModel> where TUser : IdentityUser where TTokenModel : UserTokenModel
    {
        Task<TTokenModel> AuthenticateAsync(string username, string password);

        Task<TTokenModel> GenerateTokenAsync(TUser user);
    }

    public interface IAuthService<TUser> : IAuthService<TUser, UserTokenModel> where TUser : IdentityUser 
    {

    }

    public class AuthService<TUser> : AuthService<TUser, UserTokenModel>, IAuthService<TUser> where TUser : IdentityUser
    {
        public AuthService(UserManager<TUser> userManager, SignInManager<TUser> signInManager, IOptions<TokenProviderOptions> options, IHttpContextAccessor httpContextAccessor) : base(userManager, signInManager, options, httpContextAccessor)
        {
        }
    }

    public class AuthService<TUser, TTokenModel> : IAuthService<TUser, TTokenModel> where TUser : IdentityUser where TTokenModel : UserTokenModel, new()
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications

        private readonly UserManager<TUser> _userManager;
        private readonly SignInManager<TUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TokenProviderOptions _options;

        public AuthService(UserManager<TUser> userManager, SignInManager<TUser> signInManager, IOptions<TokenProviderOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public async Task<TTokenModel> AuthenticateAsync(string username, string password)
        {
            TUser user = null;

            try
            {
                user = await GetIdentity(username, password);
            }
            catch (Exception e)
            {

            }

            if (user == null)
            {
                throw new Exception("Invalid username or password.");
            }

            return await GenerateTokenAsync(user);
        }



        public async Task<TTokenModel> GenerateTokenAsync(TUser user)
        {
            var now = DateTime.UtcNow;
            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            // You can add other claims here, if you want:
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ApplicationClaimTypes.PhoneConfirmed, user.PhoneNumberConfirmed.ToString()),
                new Claim(ApplicationClaimTypes.HasPassword, (await _userManager.HasPasswordAsync(user)).ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(Enumerable.Select<string, Claim>(roles, r => new Claim(ClaimTypes.Role, r)));

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials);
            var jwtHandler = new JwtSecurityTokenHandler();
            jwtHandler.InboundClaimTypeMap[JwtRegisteredClaimNames.Sub] = ClaimTypes.Name;
            var encodedJwt = jwtHandler.WriteToken(jwt);

            var response = new TTokenModel
            {
                Token = encodedJwt,
                //expires_in = (int)_options.Expiration.TotalSeconds,
                //FullName = user.FullName,
                //Role = string.Join(",", roles)
            };

            return response;
        }

        protected async Task<TUser> GetIdentity(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                string normalizedPhoneNumber = FormatHelpers.NormalizePhoneNumber(username);
                if (Regex.IsMatch(normalizedPhoneNumber, ValidationPatternConstants.PhoneNumber))
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhoneNumber);
                }
            }

            if (user == null)
            {
                return null;
            }

            var checkPasswordResult = await _signInManager.CheckPasswordSignInAsync(user, password, true);

            if (checkPasswordResult.Succeeded)
            {
                return user;
            }

            // Credentials are invalid, or account doesn't exist
            return null;
        }
    }
}