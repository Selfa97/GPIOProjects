namespace GPIOModels.Configuration
{
    public class JWTConfig
    {
        public string Key { get; }
        public string Issuer { get; }
        public int ExpiryInMinutes { get; }
    }
}