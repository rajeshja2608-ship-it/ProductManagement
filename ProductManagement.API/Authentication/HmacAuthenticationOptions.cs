using Microsoft.AspNetCore.Authentication;

namespace ProductManagement.API.Authentication
{
    public class HmacAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "Hmac";
        public int ExpirySeconds { get; set; } = 300;
    }
}
