namespace Users.Application.Services
{
    public class JwtSettings
    {
        public string Secret { get; set; } = "SuperLongSecretKey1234567890123456";
        public string Issuer { get; set; } = "MyAuthServer";
        public int ExpirationInMinutes { get; set; } = 60;
        public string Audience { get; set; } = string.Empty;
    }
}
