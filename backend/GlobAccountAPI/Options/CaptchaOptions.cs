namespace GlobAccountAPI.Options
{
    public class CaptchaOptions
    {
        public bool Enabled { get; set; }
        public string SecretKey { get; set; } = string.Empty;
        public string VerificationUrl { get; set; } = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
    }
}
