using System.Text;

namespace Server.Reawakened.Players.Helpers;

public class SeparatedStringBuilder(char separator)
{
    private readonly StringBuilder _stringBuilder = new();
    private bool _first = true;

    public void Append(object text)
    {
        if (!_first)
            _stringBuilder.Append(separator);
        else
            _first = false;

        _stringBuilder.Append(text.ToString());
    }

    public override string ToString() => _stringBuilder.ToString();
}
