using Server.Base.Core.Abstractions;

namespace Web.Razor.Configs;
public class WebsiteRwConfig : IRwConfig
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPass { get; set; }
    public string SenderAddress { get; set; }
    public string DownloadName { get; set; }

    public WebsiteRwConfig()
    {
        SmtpServer = "smtp.example.com";
        SmtpPort = 587;
        SmtpUser = "your-username";
        SmtpPass = "your-password";
        SenderAddress = "mail@example.com";
    }
}
