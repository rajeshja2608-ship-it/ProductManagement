using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ProductManagement.API.Authentication
{
    public class HmacAuthenticationHandler
        : AuthenticationHandler<HmacAuthenticationOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly IHmacNonceStore _nonceStore;

        public HmacAuthenticationHandler(
            IOptionsMonitor<HmacAuthenticationOptions> options,
            ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder,
            IConfiguration configuration,
            IHmacNonceStore nonceStore)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
            _nonceStore = nonceStore;
        }

        protected override async Task<AuthenticateResult>
            HandleAuthenticateAsync()
        {
            try
            {
                // ==========================================
                // 1. GET HEADERS
                // ==========================================

                if (!Request.Headers.TryGetValue(
                    "X-Client-Id",
                    out var clientIdValues))
                {
                    return AuthenticateResult.Fail(
                        "Missing X-Client-Id.");
                }

                if (!Request.Headers.TryGetValue(
                    "X-Timestamp",
                    out var timestampValues))
                {
                    return AuthenticateResult.Fail(
                        "Missing X-Timestamp.");
                }

                if (!Request.Headers.TryGetValue(
                    "X-Nonce",
                    out var nonceValues))
                {
                    return AuthenticateResult.Fail(
                        "Missing X-Nonce.");
                }

                if (!Request.Headers.TryGetValue(
                    "X-Signature",
                    out var signatureValues))
                {
                    return AuthenticateResult.Fail(
                        "Missing X-Signature.");
                }


                string clientId =
                    clientIdValues.ToString();

                string timestampString =
                    timestampValues.ToString();

                string nonce =
                    nonceValues.ToString();

                string providedSignature =
                    signatureValues.ToString();


                // ==========================================
                // 2. GET SECRET
                // ==========================================

                string? secret =
                    _configuration[
                        $"Hmac:Clients:{clientId}:Secret"];

                if (string.IsNullOrWhiteSpace(secret))
                {
                    return AuthenticateResult.Fail(
                        "Invalid HMAC client.");
                }


                // ==========================================
                // 3. VALIDATE TIMESTAMP
                // ==========================================

                if (!long.TryParse(
                    timestampString,
                    out long timestamp))
                {
                    return AuthenticateResult.Fail(
                        "Invalid timestamp.");
                }

                DateTimeOffset requestTime;

                try
                {
                    requestTime =
                        DateTimeOffset.FromUnixTimeSeconds(
                            timestamp);
                }
                catch
                {
                    return AuthenticateResult.Fail(
                        "Invalid timestamp.");
                }


                DateTimeOffset currentTime =
                    DateTimeOffset.UtcNow;

                double difference =
                    Math.Abs(
                        (currentTime - requestTime)
                        .TotalSeconds);

                if (difference > Options.ExpirySeconds)
                {
                    return AuthenticateResult.Fail(
                        "HMAC request expired.");
                }


                // ==========================================
                // 4. VALIDATE NONCE
                // ==========================================

                if (string.IsNullOrWhiteSpace(nonce))
                {
                    return AuthenticateResult.Fail(
                        "Invalid nonce.");
                }

                DateTimeOffset nonceExpiry =
                    currentTime.AddSeconds(
                        Options.ExpirySeconds);

                bool nonceAccepted =
                    _nonceStore.TryUseNonce(
                        clientId,
                        nonce,
                        nonceExpiry);

                if (!nonceAccepted)
                {
                    return AuthenticateResult.Fail(
                        "Nonce already used.");
                }


                // ==========================================
                // 5. READ BODY
                // ==========================================

                string body = string.Empty;

                if (Request.ContentLength > 0)
                {
                    Request.EnableBuffering();

                    Request.Body.Position = 0;

                    using var reader =
                        new StreamReader(
                            Request.Body,
                            Encoding.UTF8,
                            leaveOpen: true);

                    body =
                        await reader.ReadToEndAsync();

                    Request.Body.Position = 0;
                }


                // ==========================================
                // 6. BODY SHA256
                // ==========================================

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


                // ==========================================
                // 7. REQUEST PATH
                // ==========================================

                string path =
                    Request.PathBase +
                    Request.Path;


                string method =
                    Request.Method
                        .ToUpperInvariant();


                // ==========================================
                // 8. CANONICAL STRING
                // ==========================================

                string canonicalString =
                    $"{clientId}\n" +
                    $"{timestamp}\n" +
                    $"{nonce}\n" +
                    $"{method}\n" +
                    $"{path}\n" +
                    $"{bodyHash}";


                // ==========================================
                // 9. CREATE HMAC
                // ==========================================

                byte[] secretBytes =
                    Encoding.UTF8.GetBytes(secret);

                byte[] messageBytes =
                    Encoding.UTF8.GetBytes(
                        canonicalString);

                using var hmac =
                    new HMACSHA256(secretBytes);

                byte[] computedHash =
                    hmac.ComputeHash(messageBytes);

                string computedSignature =
                    Convert.ToBase64String(
                        computedHash);


                // ==========================================
                // 10. COMPARE SIGNATURE
                // ==========================================

                bool signatureValid =
                    CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(
                            computedSignature),

                        Encoding.UTF8.GetBytes(
                            providedSignature));

                if (!signatureValid)
                {
                    return AuthenticateResult.Fail(
                        "Invalid HMAC signature.");
                }


                // ==========================================
                // 11. CREATE HMAC CLAIMS
                // ==========================================

                var claims = new List<Claim>
                {
                    new Claim(
                        "client_id",
                        clientId),

                    new Claim(
                        "auth_type",
                        "HMAC"),

                    new Claim(
                        ClaimTypes.AuthenticationMethod,
                        "HMAC")
                };


                var identity =
                    new ClaimsIdentity(
                        claims,
                        Scheme.Name);

                var principal =
                    new ClaimsPrincipal(identity);

                var ticket =
                    new AuthenticationTicket(
                        principal,
                        Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "HMAC authentication failed.");

                return AuthenticateResult.Fail(
                    "HMAC authentication failed.");
            }
        }

        protected override Task HandleChallengeAsync(
            AuthenticationProperties properties)
        {
            Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        }
    }
}