using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace ProductManagement.UI.Security
{
    public class HmacHttpMessageHandler : DelegatingHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HmacHttpMessageHandler(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // =====================================================
            // 1. GET HMAC CONFIGURATION
            // =====================================================

            string clientId =
                _configuration["Hmac:ClientId"] ?? string.Empty;

            string secret =
                _configuration["Hmac:Secret"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new InvalidOperationException(
                    "Hmac:ClientId is not configured.");
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "Hmac:Secret is not configured.");
            }


            // =====================================================
            // 2. GET JWT FROM SESSION
            // =====================================================

            string? jwtToken =
                _httpContextAccessor
                    .HttpContext?
                    .Session
                    .GetString("JwtToken");


            // =====================================================
            // 3. ADD JWT
            // =====================================================

            if (!string.IsNullOrWhiteSpace(jwtToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        jwtToken);
            }


            // =====================================================
            // 4. TIMESTAMP
            // =====================================================

            long timestamp =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds();


            // =====================================================
            // 5. NONCE
            // =====================================================

            string nonce =
                Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(32));


            // =====================================================
            // 6. REQUEST BODY
            // =====================================================

            string body = string.Empty;

            if (request.Content != null)
            {
                body =
                    await request.Content.ReadAsStringAsync(
                        cancellationToken);
            }


            // =====================================================
            // 7. SHA256 BODY HASH
            // =====================================================

            string bodyHash;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bodyBytes =
                    Encoding.UTF8.GetBytes(body);

                byte[] hash =
                    sha256.ComputeHash(bodyBytes);

                bodyHash =
                    Convert.ToHexString(hash)
                        .ToLowerInvariant();
            }


            // =====================================================
            // 8. HTTP METHOD
            // =====================================================

            string method =
                request.Method
                    .Method
                    .ToUpperInvariant();


            // =====================================================
            // 9. REQUEST PATH
            // =====================================================

            string path =
                request.RequestUri?.AbsolutePath
                ?? string.Empty;


            // =====================================================
            // 10. CANONICAL STRING
            // =====================================================

            string canonicalString =
                $"{clientId}\n" +
                $"{timestamp}\n" +
                $"{nonce}\n" +
                $"{method}\n" +
                $"{path}\n" +
                $"{bodyHash}";


            // =====================================================
            // 11. CREATE HMAC SHA256
            // =====================================================

            byte[] secretBytes =
                Encoding.UTF8.GetBytes(secret);

            byte[] messageBytes =
                Encoding.UTF8.GetBytes(
                    canonicalString);

            byte[] signatureBytes;

            using (var hmac =
                   new HMACSHA256(secretBytes))
            {
                signatureBytes =
                    hmac.ComputeHash(messageBytes);
            }


            string signature =
                Convert.ToBase64String(
                    signatureBytes);


            // =====================================================
            // 12. REMOVE OLD HMAC HEADERS
            // =====================================================

            request.Headers.Remove("X-Client-Id");
            request.Headers.Remove("X-Timestamp");
            request.Headers.Remove("X-Nonce");
            request.Headers.Remove("X-Signature");


            // =====================================================
            // 13. ADD HMAC HEADERS
            // =====================================================

            request.Headers.Add(
                "X-Client-Id",
                clientId);

            request.Headers.Add(
                "X-Timestamp",
                timestamp.ToString());

            request.Headers.Add(
                "X-Nonce",
                nonce);

            request.Headers.Add(
                "X-Signature",
                signature);


            // =====================================================
            // 14. SEND REQUEST
            // =====================================================

            return await base.SendAsync(
                request,
                cancellationToken);
        }
    }
}