namespace GlobAccountAPI.Services
{
    public sealed record CaptchaVerificationResult(bool IsValid, string? Reason = null)
    {
        public static CaptchaVerificationResult Success() => new(true);

        public static CaptchaVerificationResult Failure(string reason) => new(false, reason);
    }
}
