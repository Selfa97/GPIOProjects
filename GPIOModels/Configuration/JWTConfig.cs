namespace GPIOModels.Configuration
{
    public class JWTConfig
    {
        public string Issuer { get; set; }
        public int ExpiryInMinutes { get; set; }
    }
}