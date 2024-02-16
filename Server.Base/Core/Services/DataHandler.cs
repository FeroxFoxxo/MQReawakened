using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Models;
using Server.Base.Worlds.EventArguments;
using System.Text.Json;

namespace Server.Base.Core.Services;

public abstract class DataHandler<T>(EventSink sink, ILogger<T> logger, InternalRConfig rConfig, InternalRwConfig rwConfig) : IService where T : PersistantData
{
    public readonly ILogger<T> Logger = logger;
    public readonly EventSink Sink = sink;
    public readonly InternalRConfig RConfig = rConfig;
    public readonly InternalRwConfig RwConfig = rwConfig;

    public Dictionary<int, T> Data = [];

    private JsonSerializerOptions jsonSerializerOptions;

    public abstract bool HasDefault { get; }

    public virtual void Initialize()
    {
        Sink.WorldLoad += Load;
        Sink.WorldSave += Save;
        Sink.CreateData += () => CreateInternal($"new {typeof(T).Name.ToLower()}");

        jsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = RwConfig.IndentSaves
        };
    }

    public string GetFileName() =>
        Path.Combine(RConfig.SaveDirectory, $"{typeof(T).Name.ToLower()}.json");

    public void Load()
    {
        try
        {
            var filePath = GetFileName();

            if (File.Exists(filePath))
            {
                using StreamReader streamReader = new(filePath, false);
                var contents = streamReader.ReadToEnd();

                Data = JsonSerializer.Deserialize<Dictionary<int, T>>(contents, jsonSerializerOptions) ??
                       throw new InvalidOperationException();

                var count = Data.Count;

                Logger.LogInformation("Loaded {Count} {Name}{Plural} to memory from {Directory}", count,
                    typeof(T).Name.ToLower(), count != 1 ? "s" : string.Empty, filePath);

                streamReader.Close();
            }
            else
            {
                Logger.LogWarning("Could not find save file for {FileName}, generating default.", filePath);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not deserialize save for {Type}.", typeof(T).Name);
        }

        if (Data.Count <= 0)
            CreateInternal("server owner");

        OnAfterLoad();
    }

    public void CreateInternal(string name)
    {
        if (!HasDefault)
            return;

        var type = typeof(T).Name.ToLower();

        Logger.LogDebug("This server does not have a(n) {Type}.", type);
        Logger.LogDebug("Please create a(n) {Type} for the {Name} now", type, name);

        var t = CreateDefault();

        if (t == null)
        {
            Logger.LogError("Failed to create {Type}.", type);
        }
        else
        {
            Add(t);
            Logger.LogDebug("Created {Name}.", type);
        }
    }

    public abstract T CreateDefault();

    public virtual void OnAfterLoad()
    {
    }

    public void Save(WorldSaveEventArgs worldSaveEventArgs)
    {
        var filePath = GetFileName();

        using StreamWriter streamWriter = new(filePath, false);

        var json = JsonSerializer.Serialize(Data, jsonSerializerOptions);

        streamWriter.Write(json);

        streamWriter.Close();
    }

    public T Get(int userId)
    {
        Data.TryGetValue(userId, out var type);

        return type;
    }

    public int CreateNewId() => Data.Count == 0 ? 1 : Data.Max(x => x.Key) + 1;

    public void Add(T entity, int id = -1)
    {
        if (id == -1)
            id = CreateNewId();
        Data.Add(id, entity);
        if (entity is PersistantData pd)
            pd.Id = id;
    }
}
