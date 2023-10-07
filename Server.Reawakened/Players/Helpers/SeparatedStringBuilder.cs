namespace Server.Reawakened.Players.Helpers;

public class SeparatedStringBuilder(char separator)
{
    private readonly char _separator = separator;
    private readonly List<string> _text = new();

    public void Append(object text) => _text.Add(text.ToString());

    public override string ToString() => string.Join(_separator, _text);
}
