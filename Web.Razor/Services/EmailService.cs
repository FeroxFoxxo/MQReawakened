using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using RazorLight;
using Server.Base.Core.Configs;
using Server.Reawakened.Core.Configs;
using Web.Razor.Configs;
using Web.Razor.Templates;

namespace Web.Razor.Services;

public class EmailService(ServerRwConfig sConfig, InternalRwConfig iConfig, EmailRwConfig config, ILogger<EmailService> logger)
{
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var template = await LoadTemplateAsync("PasswordResetTemplate.cshtml");

        var model = new UsernameResetTemplateModel(sConfig, iConfig) { ResetLink = resetLink };
        var body = await RenderTemplateAsync(template, model);

        await SendEmail("Username Reset Request", body, toEmail);

        logger.LogDebug("Password reset email sent successfully to mail: {Mail}.", toEmail);
    }

    public async Task SendUsernameResetEmailAsync(string toEmail, string resetLink)
    {
        var template = await LoadTemplateAsync("UsernameResetTemplate.cshtml");

        var model = new UsernameResetTemplateModel(sConfig, iConfig) { ResetLink = resetLink };
        var body = await RenderTemplateAsync(template, model);

        await SendEmail("Username Reset Request", body, toEmail);

        logger.LogDebug("Username reset email sent successfully to mail: {Mail}.", toEmail);
    }

    private async Task SendEmail(string subject, string body, string toEmail)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(iConfig.ServerName, config.SmtpUser));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(config.SmtpServer, config.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(config.SmtpUser, config.SmtpPass);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    private static async Task<string> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", templateName);
        return await File.ReadAllTextAsync(templatePath);
    }

    private static async Task<string> RenderTemplateAsync(string template, object model)
    {
        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(EmailService))
            .UseMemoryCachingProvider()
            .Build();

        return await engine.CompileRenderStringAsync("templateKey", template, model);
    }
}
