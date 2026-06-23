using System.Net;
using System.Net.Mail;
using System.Text;
using GlobAccountAPI.DTOs;

namespace GlobAccountAPI.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendContactEmailAsync(ContactRequest request, CancellationToken cancellationToken)
        {
            var host = GetRequiredSetting("Smtp:Host");
            var port = GetRequiredIntSetting("Smtp:Port");
            var username = GetRequiredEmailSetting("Smtp:Username");
            var password = GetRequiredSetting("Smtp:Password");
            var recipientEmail = GetRequiredEmailSetting("Smtp:RecipientEmail");
            var enableSsl = GetRequiredBoolSetting("Smtp:EnableSsl");
            var timeoutSeconds = GetRequiredIntSetting("Smtp:TimeoutSeconds");
            var senderEmail = request.Email.Trim();
            var senderName = request.Name.Trim();

            if (timeoutSeconds <= 0)
            {
                throw new InvalidOperationException("La configuracion Smtp:TimeoutSeconds debe ser mayor a 0.");
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            using var message = new MailMessage
            {
                From = new MailAddress(username),
                Subject = "Nuevo mensaje desde formulario web",
                Body = BuildContactEmailBody(request),
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = false
            };

            message.To.Add(recipientEmail);
            message.ReplyToList.Add(CreateEmailAddress(senderEmail, "Email", senderName));

            using var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = (int)TimeSpan.FromSeconds(timeoutSeconds).TotalMilliseconds
            };

            try
            {
                _logger.LogInformation(
                    "Enviando correo de contacto via SMTP. Host: {Host}, Port: {Port}, EnableSsl: {EnableSsl}, MessageLength: {MessageLength}, HasPhone: {HasPhone}",
                    host,
                    port,
                    enableSsl,
                    request.Message.Length,
                    !string.IsNullOrWhiteSpace(request.Phone));

                await smtpClient.SendMailAsync(message, timeoutCts.Token);

                _logger.LogInformation("Correo de contacto enviado correctamente via SMTP.");
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "Tiempo de espera agotado enviando correo de contacto via SMTP. Host: {Host}, Port: {Port}, TimeoutSeconds: {TimeoutSeconds}",
                    host,
                    port,
                    timeoutSeconds);

                throw new TimeoutException("Se agoto el tiempo de espera al enviar el correo de contacto.", ex);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(
                    ex,
                    "Error SMTP enviando correo de contacto. StatusCode: {StatusCode}, Host: {Host}, Port: {Port}, EnableSsl: {EnableSsl}",
                    ex.StatusCode,
                    host,
                    port,
                    enableSsl);

                throw;
            }
        }

        private static string BuildContactEmailBody(ContactRequest request)
        {
            var phone = string.IsNullOrWhiteSpace(request.Phone)
                ? "No proporcionado"
                : request.Phone.Trim();

            var body = new StringBuilder()
                .AppendLine($"Nombre: {request.Name.Trim()}")
                .AppendLine($"Email: {request.Email.Trim()}")
                .AppendLine($"Telefono: {phone}")
                .AppendLine("Mensaje:")
                .AppendLine(request.Message.Trim());

            return body.ToString();
        }

        private string GetRequiredSetting(string key)
        {
            var value = _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Falta configurar {key}.");
            }

            return value;
        }

        private string GetRequiredEmailSetting(string key)
        {
            var value = GetRequiredSetting(key);

            if (!MailAddress.TryCreate(value, out var mailAddress)
                || string.IsNullOrWhiteSpace(mailAddress.Address))
            {
                throw new InvalidOperationException($"La configuracion {key} debe ser un correo valido.");
            }

            return mailAddress.Address;
        }

        private static MailAddress CreateEmailAddress(string email, string fieldName, string? displayName = null)
        {
            if (!MailAddress.TryCreate(email, out var mailAddress)
                || string.IsNullOrWhiteSpace(mailAddress.Address))
            {
                throw new InvalidOperationException($"El campo {fieldName} debe ser un correo valido.");
            }

            return string.IsNullOrWhiteSpace(displayName)
                ? mailAddress
                : new MailAddress(mailAddress.Address, displayName.Trim());
        }

        private int GetRequiredIntSetting(string key)
        {
            var value = GetRequiredSetting(key);

            if (!int.TryParse(value, out var parsedValue))
            {
                throw new InvalidOperationException($"La configuracion {key} debe ser un numero valido.");
            }

            return parsedValue;
        }

        private bool GetRequiredBoolSetting(string key)
        {
            var value = GetRequiredSetting(key);

            if (!bool.TryParse(value, out var parsedValue))
            {
                throw new InvalidOperationException($"La configuracion {key} debe ser true o false.");
            }

            return parsedValue;
        }
    }
}
