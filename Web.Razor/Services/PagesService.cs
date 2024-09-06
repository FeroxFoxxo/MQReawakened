using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using RazorLight;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Reawakened.Core.Configs;
using Web.Razor.Configs;
using Web.Razor.EmailTemplates;

namespace Web.Razor.Services;

public class PagesService(InternalRwConfig iConfig, ServerRConfig sConfig,
    EmailRwConfig config, ILogger<PagesService> logger, EventSink sink) : IService
{
    public string ZipPath { get; set; }

    public void Initialize() => sink.WorldLoad += CreateReawakened;

    public void CreateReawakened()
    {
        foreach (var file in Directory.GetFiles(sConfig.DownloadDirectory, "*__"))
            File.Delete(file);

        var name = $"Play{iConfig.ServerName}.zip";

        File.Create(Path.Join(sConfig.DownloadDirectory, $"__Place {name} Here__"));

        ZipPath = Path.Join(sConfig.DownloadDirectory, name);
    }

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
            .UseEmbeddedResourcesProject(typeof(PagesService).Assembly, "Web.Razor.EmailTemplates")
            .UseMemoryCachingProvider()
            .Build();

        return await engine.CompileRenderAsync(template, model);
    }

    public static async Task Delay() => await Task.Delay(new Random().Next(100, 500));
}
