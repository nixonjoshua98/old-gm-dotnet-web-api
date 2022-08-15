using Microsoft.IdentityModel.Tokens;

namespace GMServer.Models.Settings
{
    public class JWTBearerSettings
    {
        public string Key { get; set; }
        public byte[] KeyBytes => System.Text.Encoding.UTF8.GetBytes(Key);


        private SymmetricSecurityKey _SymmetricSecurityKey;
        public SymmetricSecurityKey SymmetricSecurityKey => _SymmetricSecurityKey ??= new SymmetricSecurityKey(KeyBytes);


        private SigningCredentials _SigningCredentials;
        public SigningCredentials SigningCredentials => _SigningCredentials ??= new(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
    }
}
