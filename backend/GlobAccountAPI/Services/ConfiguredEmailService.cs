using GlobAccountAPI.DTOs;
using GlobAccountAPI.Options;
using Microsoft.Extensions.Options;

namespace GlobAccountAPI.Services
{
    public class ConfiguredEmailService : IEmailService
    {
        private const string ApiProvider = "Api";
        private const string SmtpProvider = "Smtp";

        private readonly EmailOptions _options;
        private readonly EmailApiService _emailApiService;
        private readonly SmtpEmailService _smtpEmailService;
        private readonly ILogger<ConfiguredEmailService> _logger;

        public ConfiguredEmailService(
            IOptions<EmailOptions> options,
            EmailApiService emailApiService,
            SmtpEmailService smtpEmailService,
            ILogger<ConfiguredEmailService> logger)
        {
            _options = options.Value;
            _emailApiService = emailApiService;
            _smtpEmailService = smtpEmailService;
            _logger = logger;
        }

        public Task SendContactEmailAsync(ContactRequest request, CancellationToken cancellationToken)
        {
            var provider = string.IsNullOrWhiteSpace(_options.Provider)
                ? SmtpProvider
                : _options.Provider.Trim();

            if (string.Equals(provider, ApiProvider, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Sending contact email using configured provider: {EmailProvider}.", ApiProvider);
                return _emailApiService.SendContactEmailAsync(request, cancellationToken);
            }

            if (string.Equals(provider, SmtpProvider, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Sending contact email using configured provider: {EmailProvider}.", SmtpProvider);
                return _smtpEmailService.SendContactEmailAsync(request, cancellationToken);
            }

            _logger.LogError("Unsupported Email:Provider configured: {EmailProvider}.", provider);
            throw new InvalidOperationException("La configuracion Email:Provider debe ser Api o Smtp.");
        }
    }
}
