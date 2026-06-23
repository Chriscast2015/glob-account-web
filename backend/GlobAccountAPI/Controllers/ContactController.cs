using GlobAccountAPI.DTOs;
using GlobAccountAPI.Options;
using GlobAccountAPI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace GlobAccountAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("ContactFormPolicy")]
    public class ContactController : ControllerBase
    {
        private const string SuccessMessage = "Mensaje enviado correctamente.";

        private readonly IEmailService _emailService;
        private readonly ICaptchaVerifier _captchaVerifier;
        private readonly ContactSecurityOptions _securityOptions;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            IEmailService emailService,
            ICaptchaVerifier captchaVerifier,
            IOptions<ContactSecurityOptions> securityOptions,
            IWebHostEnvironment environment,
            ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _captchaVerifier = captchaVerifier;
            _securityOptions = securityOptions.Value;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> SendContactMessage(
            [FromBody] ContactRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(item => item.Value?.Errors.Count > 0)
                    .ToDictionary(
                        item => item.Key,
                        item => item.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                return BadRequest(new
                {
                    success = false,
                    message = "La solicitud contiene campos invalidos.",
                    errors
                });
            }

            if (!string.IsNullOrWhiteSpace(request.Website))
            {
                _logger.LogWarning("Formulario de contacto descartado por honeypot.");

                return Ok(new
                {
                    success = true,
                    message = SuccessMessage
                });
            }

            if (WasSubmittedTooQuickly(request.StartedAt))
            {
                _logger.LogWarning("Formulario de contacto rechazado por envio demasiado rapido.");

                return BadRequest(new
                {
                    success = false,
                    message = "No pudimos validar el formulario. Intentalo nuevamente."
                });
            }

            var captchaResult = await _captchaVerifier.VerifyAsync(
                request.TurnstileToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            if (!captchaResult.IsValid)
            {
                _logger.LogWarning("Captcha de contacto rechazado: {Reason}", captchaResult.Reason);

                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "No pudimos validar que la solicitud sea legitima."
                });
            }

            try
            {
                await _emailService.SendContactEmailAsync(request, cancellationToken);

                return Ok(new
                {
                    success = true,
                    message = SuccessMessage
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error de configuracion de correo al enviar mensaje de contacto.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = _environment.IsDevelopment()
                        ? ex.Message
                        : "El servicio de correo no esta configurado correctamente."
                });
            }
            catch (EmailDeliveryException ex)
            {
                _logger.LogError(ex, "Error del proveedor de correo al enviar mensaje de contacto.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "No se pudo enviar el mensaje en este momento."
                });
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Error SMTP al enviar mensaje de contacto.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "No se pudo enviar el mensaje en este momento."
                });
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout al enviar mensaje de contacto.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "No se pudo enviar el mensaje en este momento."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al enviar mensaje de contacto.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "No se pudo enviar el mensaje en este momento."
                });
            }
        }

        private bool WasSubmittedTooQuickly(long? startedAt)
        {
            if (!startedAt.HasValue || _securityOptions.MinimumCompletionSeconds <= 0)
            {
                return false;
            }

            var elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startedAt.Value;
            return elapsed < TimeSpan.FromSeconds(_securityOptions.MinimumCompletionSeconds).TotalMilliseconds;
        }
    }
}
