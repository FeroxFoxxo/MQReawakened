using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using RazorLight;
using Server.Base.Core.Configs;
using Server.Reawakened.Core.Configs;
using Web.Razor.Configs;
using Web.Razor.EmailTemplates;

namespace Web.Razor.Services;

public class EmailService(InternalRwConfig iConfig, EmailRwConfig config, ILogger<EmailService> logger)
{
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string username)
    {
        var model = new PasswordResetModel(iConfig) { ResetLink = resetLink, Username = username };
        var body = await RenderTemplateAsync("PasswordReset", model);
        await SendEmail("Password Reset Request", body, toEmail);
        logger.LogDebug("Password reset email sent successfully to mail: '{Mail}'", toEmail);
    }

    public async Task SendUsernameResetEmailAsync(string toEmail, string resetLink, string email)
    {
        var model = new UsernameResetModel(iConfig) { ResetLink = resetLink, Email = email };
        var body = await RenderTemplateAsync("UsernameReset", model);
        await SendEmail("Username Reset Request", body, toEmail);
        logger.LogDebug("Username reset email sent successfully to mail: '{Mail}'", toEmail);
    }

    private async Task SendEmail(string subject, string body, string toEmail)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(iConfig.ServerName, config.SenderAddress));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        email.Body = new BodyBuilder()
        {
            HtmlBody = body
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(config.SmtpServer, config.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(config.SmtpUser, config.SmtpPass);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    private static async Task<string> RenderTemplateAsync(string template, object model)
    {
        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(EmailService).Assembly, "Web.Razor.EmailTemplates")
            .UseMemoryCachingProvider()
            .Build();

        return await engine.CompileRenderAsync(template, model);
    }

    public static async Task Delay() => await Task.Delay(new Random().Next(100, 500));
}
