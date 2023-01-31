namespace Server.Reawakened.Players.Helpers;

public class SeparatedStringBuilder
{
    private readonly char _separator;
    private readonly List<string> _text;

    public SeparatedStringBuilder(char separator)
    {
        _separator = separator;
        _text = new List<string>();
    }

    public void Append(object text) => _text.Add(text.ToString());

    public override string ToString() => string.Join(_separator, _text);
}
