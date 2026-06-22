using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Serialization;
using GlobAccountAPI.DTOs;
using GlobAccountAPI.Options;
using Microsoft.Extensions.Options;

namespace GlobAccountAPI.Services
{
    public class EmailApiService : IEmailService
    {
        private const string ResendProvider = "Resend";
        private const string Subject = "Nuevo mensaje desde Global Account Services.";

        private readonly HttpClient _httpClient;
        private readonly EmailApiOptions _options;
        private readonly ILogger<EmailApiService> _logger;

        public EmailApiService(
            HttpClient httpClient,
            IOptions<EmailApiOptions> options,
            ILogger<EmailApiService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendContactEmailAsync(ContactRequest request, CancellationToken cancellationToken)
        {
            ValidateOptions(_options);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var endpoint = new Uri(_options.Endpoint);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(BuildResendPayload(request, _options))
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                _logger.LogInformation(
                    "Sending contact email via API provider {EmailApiProvider}. EndpointHost: {EndpointHost}, MessageLength: {MessageLength}, HasPhone: {HasPhone}",
                    _options.Provider,
                    endpoint.Host,
                    request.Message.Length,
                    !string.IsNullOrWhiteSpace(request.Phone));

                using var response = await _httpClient.SendAsync(requestMessage, timeoutCts.Token);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Contact email sent via API provider {EmailApiProvider}. StatusCode: {StatusCode}",
                        _options.Provider,
                        (int)response.StatusCode);

                    return;
                }

                var responseBody = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                var responseSummary = SummarizeProviderResponse(responseBody, _options.ApiKey);

                _logger.LogError(
                    "Email API provider {EmailApiProvider} failed. StatusCode: {StatusCode}, ResponseSummary: {ResponseSummary}",
                    _options.Provider,
                    (int)response.StatusCode,
                    responseSummary);

                throw new EmailDeliveryException("El proveedor de correo rechazo el envio.");
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "Timeout sending contact email via API provider {EmailApiProvider}. TimeoutSeconds: {TimeoutSeconds}",
                    _options.Provider,
                    _options.TimeoutSeconds);

                throw new TimeoutException("Se agoto el tiempo de espera al enviar el correo de contacto.", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "HTTP error sending contact email via API provider {EmailApiProvider}.",
                    _options.Provider);

                throw new EmailDeliveryException("No se pudo contactar al proveedor de correo.", ex);
            }
        }

        private static void ValidateOptions(EmailApiOptions options)
        {
            if (!string.Equals(options.Provider, ResendProvider, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La configuracion EmailApi:Provider debe ser Resend.");
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("Falta configurar EmailApi:ApiKey.");
            }

            if (string.IsNullOrWhiteSpace(options.Endpoint)
                || !Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var endpoint)
                || endpoint.Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidOperationException("La configuracion EmailApi:Endpoint debe ser una URL HTTPS valida.");
            }

            if (string.IsNullOrWhiteSpace(options.FromEmail)
                || !MailAddress.TryCreate(options.FromEmail, out _))
            {
                throw new InvalidOperationException("Falta configurar EmailApi:FromEmail con un correo valido.");
            }

            if (string.IsNullOrWhiteSpace(options.ToEmail)
                || !MailAddress.TryCreate(options.ToEmail, out _))
            {
                throw new InvalidOperationException("Falta configurar EmailApi:ToEmail con un correo valido.");
            }

            if (options.TimeoutSeconds <= 0)
            {
                throw new InvalidOperationException("La configuracion EmailApi:TimeoutSeconds debe ser mayor a 0.");
            }
        }

        private static ResendEmailRequest BuildResendPayload(ContactRequest request, EmailApiOptions options)
        {
            var fromName = string.IsNullOrWhiteSpace(options.FromName)
                ? "Global Account Services"
                : options.FromName.Trim();
            var from = CreateEmailAddress(options.FromEmail, "EmailApi:FromEmail", fromName).ToString();
            var replyTo = CreateEmailAddress(request.Email.Trim(), "Email", request.Name.Trim()).ToString();
            var sentAt = DateTimeOffset.UtcNow;

            return new ResendEmailRequest(
                From: from,
                To: new[] { options.ToEmail.Trim() },
                Subject: Subject,
                Html: BuildHtmlBody(request, sentAt),
                Text: BuildTextBody(request, sentAt),
                ReplyTo: replyTo);
        }

        private static string BuildHtmlBody(ContactRequest request, DateTimeOffset sentAt)
        {
            var name = HtmlEncode(request.Name.Trim());
            var email = HtmlEncode(request.Email.Trim());
            var phone = HtmlEncode(string.IsNullOrWhiteSpace(request.Phone) ? "No proporcionado" : request.Phone.Trim());
            var message = HtmlEncodeMultiline(request.Message.Trim());
            var date = HtmlEncode(sentAt.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));

            return $"""
                <!doctype html>
                <html lang="es">
                <body>
                    <h2>Nuevo mensaje desde Global Account Services</h2>
                    <p><strong>Fecha:</strong> {date}</p>
                    <p><strong>Nombre:</strong> {name}</p>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Telefono:</strong> {phone}</p>
                    <p><strong>Mensaje:</strong></p>
                    <p>{message}</p>
                </body>
                </html>
                """;
        }

        private static string BuildTextBody(ContactRequest request, DateTimeOffset sentAt)
        {
            var phone = string.IsNullOrWhiteSpace(request.Phone)
                ? "No proporcionado"
                : request.Phone.Trim();

            return new StringBuilder()
                .AppendLine("Nuevo mensaje desde Global Account Services")
                .AppendLine($"Fecha: {sentAt:yyyy-MM-dd HH:mm:ss 'UTC'}")
                .AppendLine($"Nombre: {request.Name.Trim()}")
                .AppendLine($"Email: {request.Email.Trim()}")
                .AppendLine($"Telefono: {phone}")
                .AppendLine("Mensaje:")
                .AppendLine(request.Message.Trim())
                .ToString();
        }

        private static MailAddress CreateEmailAddress(string email, string fieldName, string? displayName = null)
        {
            if (!MailAddress.TryCreate(email, out var mailAddress)
                || string.IsNullOrWhiteSpace(mailAddress.Address))
            {
                throw new InvalidOperationException($"La configuracion {fieldName} debe ser un correo valido.");
            }

            return string.IsNullOrWhiteSpace(displayName)
                ? mailAddress
                : new MailAddress(mailAddress.Address, displayName.Trim());
        }

        private static string HtmlEncode(string value)
        {
            return WebUtility.HtmlEncode(value);
        }

        private static string HtmlEncodeMultiline(string value)
        {
            return HtmlEncode(value).Replace("\r\n", "\n").Replace("\n", "<br>");
        }

        private static string SummarizeProviderResponse(string? responseBody, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return "empty";
            }

            var summary = responseBody
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                summary = summary.Replace(apiKey, "[redacted]", StringComparison.Ordinal);
            }

            return summary.Length <= 300
                ? summary
                : summary[..300] + "...";
        }

        private sealed record ResendEmailRequest(
            [property: JsonPropertyName("from")] string From,
            [property: JsonPropertyName("to")] string[] To,
            [property: JsonPropertyName("subject")] string Subject,
            [property: JsonPropertyName("html")] string Html,
            [property: JsonPropertyName("text")] string Text,
            [property: JsonPropertyName("reply_to")] string ReplyTo);
    }
}
