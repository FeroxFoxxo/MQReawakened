﻿namespace Server.Reawakened.Players.Helpers;

public class SeparatedStringBuilder(char separator)
{
    private readonly List<string> _text = [];

    public void Append(object text) => _text.Add(text.ToString());

    public override string ToString() => string.Join(separator, _text);
}
