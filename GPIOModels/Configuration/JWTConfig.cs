namespace GPIOModels.Configuration
{
    public class JWTConfig
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public int ExpiryInMinutes { get; set; }
    }
}