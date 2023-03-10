using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Models;
using Server.Base.Worlds.EventArguments;

namespace Server.Base.Core.Services;

public abstract class DataHandler<T> : IService where T : PersistantData
{
    public readonly ILogger<T> Logger;
    public readonly EventSink Sink;
    public readonly InternalRConfig Config;

    public Dictionary<int, T> Data;

    protected DataHandler(EventSink sink, ILogger<T> logger, InternalRConfig config)
    {
        Sink = sink;
        Logger = logger;
        Config = config;
        Data = new Dictionary<int, T>();
    }

    public virtual void Initialize()
    {
        Sink.WorldLoad += Load;
        Sink.WorldSave += Save;
        Sink.CreateData += () => CreateInternal($"new {typeof(T).Name.ToLower()}");
    }

    public string GetFileName() =>
        Path.Combine(Config.SaveDirectory, $"{typeof(T).Name.ToLower()}.json");

    public void Load()
    {
        try
        {
            var filePath = GetFileName();

            if (File.Exists(filePath))
            {
                using StreamReader streamReader = new(filePath, false);
                var contents = streamReader.ReadToEnd();

                Data = JsonConvert.DeserializeObject<Dictionary<int, T>>(contents) ??
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
            Data.Add(Data.Count, t);
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

        streamWriter.Write(JsonConvert.SerializeObject(Data, Formatting.Indented));

        streamWriter.Close();
    }

    public T Get(int userId)
    {
        Data.TryGetValue(userId, out var type);

        return type;
    }
}
