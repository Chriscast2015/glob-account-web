namespace GlobAccountAPI.Options
{
    public class ContactSecurityOptions
    {
        public int MaxRequestBodyBytes { get; set; } = 16 * 1024;
        public int MinimumCompletionSeconds { get; set; } = 3;
    }
}
