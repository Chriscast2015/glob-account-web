using GlobAccountAPI.DTOs;

namespace GlobAccountAPI.Services
{
    public interface IEmailService
    {
        Task SendContactEmailAsync(ContactRequest request, CancellationToken cancellationToken);
    }
}
