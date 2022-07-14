namespace GMServer.Models.Settings
{
    public class JWTBearerSettings
    {
        public string Key { get; set; }

        public byte[] KeyBytes => System.Text.Encoding.UTF8.GetBytes(Key);
    }
}
