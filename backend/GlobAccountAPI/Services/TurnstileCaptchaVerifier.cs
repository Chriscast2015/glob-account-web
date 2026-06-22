using System.Text.Json.Serialization;
using System.Net.Http.Json;
using GlobAccountAPI.Options;
using Microsoft.Extensions.Options;

namespace GlobAccountAPI.Services
{
    public class TurnstileCaptchaVerifier : ICaptchaVerifier
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<CaptchaOptions> _options;
        private readonly ILogger<TurnstileCaptchaVerifier> _logger;

        public TurnstileCaptchaVerifier(
            HttpClient httpClient,
            IOptions<CaptchaOptions> options,
            ILogger<TurnstileCaptchaVerifier> logger)
        {
            _httpClient = httpClient;
            _options = options;
            _logger = logger;
        }

        public async Task<CaptchaVerificationResult> VerifyAsync(
            string? token,
            string? remoteIp,
            CancellationToken cancellationToken)
        {
            var options = _options.Value;
            var secretConfigured = !string.IsNullOrWhiteSpace(options.SecretKey);

            _logger.LogInformation(
                "Captcha configuration. Enabled: {CaptchaEnabled}, SecretConfigured: {CaptchaSecretConfigured}",
                options.Enabled,
                secretConfigured);

            if (!options.Enabled)
            {
                _logger.LogInformation("Captcha disabled; skipping contact captcha validation.");
                return CaptchaVerificationResult.Success();
            }

            if (!secretConfigured)
            {
                _logger.LogError("Captcha enabled but Captcha:SecretKey is not configured.");
                return CaptchaVerificationResult.Failure("Captcha no configurado.");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Captcha enabled but request did not include a Turnstile token.");
                return CaptchaVerificationResult.Failure("Captcha requerido.");
            }

            var fields = new List<KeyValuePair<string, string>>
            {
                new("secret", options.SecretKey),
                new("response", token)
            };

            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                fields.Add(new("remoteip", remoteIp));
            }

            using var content = new FormUrlEncodedContent(fields);

            try
            {
                using var response = await _httpClient.PostAsync(
                    options.VerificationUrl,
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Captcha devolvio estado HTTP {StatusCode}.",
                        (int)response.StatusCode);

                    return CaptchaVerificationResult.Failure("Captcha no disponible.");
                }

                var verification = await response.Content.ReadFromJsonAsync<TurnstileResponse>(
                    cancellationToken: cancellationToken);

                if (verification?.Success == true)
                {
                    return CaptchaVerificationResult.Success();
                }

                _logger.LogWarning(
                    "Captcha rechazado. ErrorCodes: {ErrorCodes}",
                    string.Join(",", verification?.ErrorCodes ?? Array.Empty<string>()));

                return CaptchaVerificationResult.Failure("Captcha invalido.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando captcha.");
                return CaptchaVerificationResult.Failure("Captcha no disponible.");
            }
        }

        private sealed class TurnstileResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("error-codes")]
            public string[]? ErrorCodes { get; set; }
        }
    }
}
