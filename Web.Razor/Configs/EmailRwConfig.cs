using Server.Base.Core.Abstractions;

namespace Web.Razor.Configs;
public class EmailRwConfig : IRwConfig
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPass { get; set; }

    public EmailRwConfig()
    {
        SmtpServer = "smtp.example.com";
        SmtpPort = 587;
        SmtpUser = "your-email@example.com";
        SmtpPass = "your-email-password";
    }
}
