using Server.Base.Accounts.Enums;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Server.Base.Core.Models;

public class InternalRConfig : IRConfig
{
    public string CrashBackupDirectory { get; set; }
    public string AutomaticBackupDirectory { get; set; }
    public string TempBackupDirectory { get; set; }
    public string SaveDirectory { get; set; }
    public string CrashDirectory { get; set; }
    public string LogDirectory { get; set; }

    public string Host { get; }
    public int Port { get; }
    public int MaxAccountsPerIp { get; }
    public char[] ForbiddenChars { get; }
    public AccessLevel LockDownLevel { get; }
    public int MaxAddresses { get; }
    public bool SocketBlock { get; }
    public int BreakCount { get; }
    public double[] Delays { get; }
    public int GlobalUpdateRange { get; }
    public int BufferSize { get; }
    public int BackupCapacity { get; }
    public int RestartWarningSeconds { get; }
    public int RestartDelaySeconds { get; }
    public int RestartAutomaticallyHours { get; }
    public int SaveWarningMinutes { get; }
    public int SaveAutomaticallyMinutes { get; }
    public string[] Backups { get; }
    public int CommandPadding { get; }

    public InternalRConfig()
    {
        CrashBackupDirectory = InternalDirectory.GetDirectory("Backups/Crashed");
        AutomaticBackupDirectory = InternalDirectory.GetDirectory("Backups/Automatic");
        TempBackupDirectory = InternalDirectory.GetDirectory("Backups/Temp");
        SaveDirectory = InternalDirectory.GetDirectory("Saves");
        LogDirectory = InternalDirectory.GetDirectory("Logs");

        MaxAccountsPerIp = 1;
        ForbiddenChars = new[]
        {
            '<', '>', ':', '"', '/', '\\', '|', '?', '*', ' ', '%'
        };
        LockDownLevel = AccessLevel.Player;
        MaxAddresses = 10;
        SocketBlock = true;
        BreakCount = 20000;
        Delays = new double[] { 0, 10, 25, 50, 250, 1000, 5000, 60000 };
        GlobalUpdateRange = 18;
        BufferSize = 4096;
        BackupCapacity = 64;
        RestartWarningSeconds = 60;
        RestartDelaySeconds = 10;
        RestartAutomaticallyHours = 24;
        SaveWarningMinutes = 1;
        SaveAutomaticallyMinutes = 5;
        CommandPadding = 8;
        Backups = new[]
        {
            "Third Backup",
            "Second Backup",
            "Most Recent"
        };
        Port = 9339;
        Host = "localhost";
    }

    public string GetHostName() => $"{Host}:{Port}";
}
