using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using RazorLight;
using Server.Base.Core.Extensions;
using System.IO.Compression;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Reawakened.Core.Configs;
using Web.Razor.Configs;
using Web.Razor.EmailTemplates;

namespace Web.Razor.Services;

public class PagesService(InternalRwConfig iConfig, ServerRConfig sConfig,
    WebsiteRwConfig config, ILogger<PagesService> logger, EventSink sink) : IService
{
    public string ZipPath { get; set; }

    public void Initialize()
    {
        sink.WorldLoad += SetupDownload;
        sink.ServerHosted += BuildDownloadIfNeeded;
    }

    public void SetupDownload()
    {
        foreach (var file in Directory.GetFiles(sConfig.DownloadDirectory, "*__"))
            File.Delete(file);

        if (string.IsNullOrEmpty(config.DownloadName))
            config.DownloadName = $"Play {iConfig.ServerName}.zip";

        var name = config.DownloadName;

        File.Create(Path.Join(sConfig.DownloadDirectory, $"__Place {name} Here__"));

        ZipPath = Path.Join(sConfig.DownloadDirectory, name);
    }

    private void BuildDownloadIfNeeded()
    {
        if (!EnvironmentExt.IsContainer())
            return;

        try
        {
            if (File.Exists(ZipPath))
            {
                logger.LogDebug("Download zip already exists at '{ZipPath}'. Skipping rebuild.", ZipPath);
                return;
            }

            var gameRoot = "/settings";
            var configFile = Path.Join(gameRoot, "game", "LocalBuildConfig.xml");
            var launcherExe = Path.Join(gameRoot, "launcher", "launcher.exe");

            var waitUntil = DateTime.UtcNow.AddSeconds(120);
            while (DateTime.UtcNow < waitUntil)
            {
                if (File.Exists(configFile) && File.Exists(launcherExe))
                    break;
                Task.Delay(1000).GetAwaiter().GetResult();
            }

            if (!File.Exists(configFile) || !File.Exists(launcherExe))
            {
                logger.LogWarning("Could not find required game files to build zip. Config: '{ConfigFile}', Launcher: '{LauncherExe}'.", configFile, launcherExe);
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ZipPath)!);
            if (File.Exists(ZipPath))
                File.Delete(ZipPath);

            logger.LogInformation("Creating downloadable game archive at '{ZipPath}' from '{GameRoot}'.", ZipPath, gameRoot);
            ZipDirectoryFiltered(gameRoot, ZipPath);
            logger.LogDebug("Game archive created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create downloadable game archive.");
        }
    }

    private static void ZipDirectoryFiltered(string sourceDir, string destinationZip)
    {
        var excludedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Thumbs.db",
            ".DS_Store",
            "desktop.ini"
        };

        using var archive = ZipFile.Open(destinationZip, ZipArchiveMode.Create);

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(file);
            if (excludedFiles.Contains(fileName))
                continue;
            if (fileName.StartsWith("._"))
                continue;

            var entryName = Path.GetRelativePath(sourceDir, file).Replace('\\', '/');

            var exclude = false;
            var idx = 0;
            while (idx < entryName.Length)
            {
                var next = entryName.IndexOf('/', idx);
                var segment = next == -1 ? entryName[idx..] : entryName[idx..next];
                if (segment.Equals("__MACOSX", StringComparison.OrdinalIgnoreCase) ||
                    segment.Equals(".git", StringComparison.OrdinalIgnoreCase) ||
                    segment.Equals(".svn", StringComparison.OrdinalIgnoreCase))
                {
                    exclude = true;
                    break;
                }
                if (next == -1) break;
                idx = next + 1;
            }
            if (exclude)
                continue;

            archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
        }
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
