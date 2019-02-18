using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RobiGroup.Web.Common.Configuration
{
    public class TokenProviderOptions
    {
        private SigningCredentials _signingCredentials = null;

        public string Path { get; set; } = "/token";

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string SigningKey { get; set; }

        public TimeSpan Expiration { get; set; } = TimeSpan.FromDays(49);

        public SigningCredentials SigningCredentials
        {
            get
            {
                if (_signingCredentials == null)
                {
                    _signingCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes((string) SigningKey)), SecurityAlgorithms.HmacSha256);
                }

                return _signingCredentials;
            }
        }
    }
}
