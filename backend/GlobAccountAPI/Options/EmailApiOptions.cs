namespace GlobAccountAPI.Options
{
    public class EmailApiOptions
    {
        public string Provider { get; set; } = "Resend";
        public string Endpoint { get; set; } = "https://api.resend.com/emails";
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Global Account Services";
        public string ToEmail { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
