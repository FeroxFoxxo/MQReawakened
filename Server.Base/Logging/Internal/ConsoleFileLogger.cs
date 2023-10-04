using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using System.Text;

namespace Server.Base.Logging.Internal;

public class ConsoleFileLogger : TextWriter
{
    public const string DateFormat = "[MMMM dd hh:mm:ss.f tt]: ";

    private bool _newLine;

    public string FileName { get; }

    public override Encoding Encoding => Encoding.Default;

    public ConsoleFileLogger(string file, InternalRConfig config)
    {
        FileName = file;

        using var writer = GetFile.GetStreamWriter(file, config.LogDirectory, FileMode.Create);
        writer.WriteLine(">>>Logging started on {0:f}.", DateTime.Now);

        _newLine = true;
    }

    public override void Write(string @string)
    {
        using var writer = GetFile.GetStreamWriter(FileName, "Logs", FileMode.Append);

        if (_newLine)
        {
            writer.Write(DateTime.UtcNow.ToString(DateFormat));
            _newLine = false;
        }

        writer.Write(@string);
    }

    public override void WriteLine(string line)
    {
        using var writer = GetFile.GetStreamWriter(FileName, "Logs", FileMode.Append);

        if (_newLine)
            writer.Write(DateTime.UtcNow.ToString(DateFormat));

        writer.WriteLine(line);
        _newLine = true;
    }
}
