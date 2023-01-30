namespace Server.Reawakened.Characters.Helpers;

public class SeparatedStringBuilder
{
    private readonly char _fieldSeparator;
    private readonly List<string> _text;

    public SeparatedStringBuilder(char fieldSeparator)
    {
        _fieldSeparator = fieldSeparator;
        _text = new List<string>();
    }

    public void Append(object text) => _text.Add(text.ToString());

    public override string ToString() => string.Join(_fieldSeparator, _text);
}
