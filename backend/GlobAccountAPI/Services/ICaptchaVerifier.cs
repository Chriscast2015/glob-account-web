namespace GlobAccountAPI.Services
{
    public interface ICaptchaVerifier
    {
        Task<CaptchaVerificationResult> VerifyAsync(
            string? token,
            string? remoteIp,
            CancellationToken cancellationToken);
    }
}
