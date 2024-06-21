using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Server.Base.Database.Abstractions;
public class CustomConverter<T> : ValueConverter<T, string> where T : class
{
    protected static readonly JsonSerializerOptions serializerOptions = new()
    {
        AllowTrailingCommas = false,
        WriteIndented = false
    };

    public CustomConverter() : base(
        v => JsonSerializer.Serialize(v, serializerOptions),
        v => JsonSerializer.Deserialize<T>(v, serializerOptions))
    { }
}
