using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.Levels.Models;
using Server.Reawakened.Levels.SyncedData.Abstractions;
using System.Runtime.Serialization;

namespace Server.Reawakened.Levels.SyncedData;

public class LevelEntities
{
    public readonly Dictionary<int, List<BaseSynchronizedEntity>> Entities;

    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public LevelEntities(Level level, Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger;

        if (level.LevelPlaneHandler.Planes == null)
            return;

        Entities = new Dictionary<int, List<BaseSynchronizedEntity>>();
        
        var invalidProcessable = new List<string>();

        var syncedEntities = typeof(BaseSynchronizedEntity).Assembly.GetServices<BaseSynchronizedEntity>()
            .Where(t => t.BaseType != null)
            .Where(t => t.BaseType.GenericTypeArguments.Length > 0)
            .ToDictionary(t => t.BaseType.GenericTypeArguments.First().FullName, t => t);
        
        foreach (var plane in level.LevelPlaneHandler.Planes)
        {
            foreach (var entity in plane.Value.GameObjects)
            {
                foreach (var component in entity.Value.ObjectInfo.Components)
                {
                    if (!level.LevelHandler.ProcessableData.TryGetValue(component.Key, out var mqType))
                        continue;

                    if (syncedEntities.TryGetValue(mqType.FullName!, out var internalType))
                    {
                        var dataObj = FormatterServices.GetUninitializedObject(mqType);

                        var fields = mqType.GetFields()
                            .Where(prop => prop.IsDefined(typeof(MQAttribute), false))
                            .ToList();

                        foreach (var componentValue in component.Value.ComponentAttributes)
                        {
                            if (string.IsNullOrEmpty(componentValue.Value))
                                continue;
                            
                            var field = fields.FirstOrDefault(f => f.Name == componentValue.Key);

                            if (field != null)
                            {
                                if (field.FieldType == typeof(string))
                                    field.SetValue(dataObj, componentValue.Value);
                                else if(field.FieldType == typeof(int))
                                    field.SetValue(dataObj, int.Parse(componentValue.Value));
                                else if (field.FieldType == typeof(bool))
                                    field.SetValue(dataObj, componentValue.Value.ToLower() == "true");
                                else
                                    _logger.LogError("It is unknown how to convert a string to a {FieldType}. " +
                                                     "Please implement this in the {CurrentType} class.", field.FieldType, GetType().Name);
                            }
                            else
                            {
                                _logger.LogWarning("Component {ComponentType} does not have field {FieldName}. Possible fields: {Fields}",
                                    mqType, componentValue.Key, string.Join(", ", fields.Select(f => f.Name)));
                            }
                        }

                        var storedData = new StoredEntityModel(entity.Key, entity.Value.ObjectInfo.PrefabName,
                            entity.Value.ObjectInfo.Position, level, logger);

                        var instancedEntity =
                            (BaseSynchronizedEntity)Activator.CreateInstance(internalType, storedData, dataObj);

                        if (!Entities.ContainsKey(entity.Key))
                            Entities.Add(entity.Key, new List<BaseSynchronizedEntity>());

                        Entities[entity.Key].Add(instancedEntity);
                    }
                    else
                    {
                        if (!invalidProcessable.Contains(mqType.Name))
                            invalidProcessable.Add(mqType.Name);
                    }
                }
            }
        }

        foreach (var type in invalidProcessable.Order())
            _logger.LogWarning("Could not find synced entity for {EntityType}", type);
    }

    public Dictionary<int, T> GetEntities<T>() where T : class
    {
        var type = typeof(T);

        var entities = Entities.Values.SelectMany(t => t).Where(t => t is T).ToList();

        if (entities.Count > 0)
            return entities.ToDictionary(x => x.StoredEntity.Id, x => x as T);
        
        _logger.LogError("Could not find entity with type {TypeName}. Returning empty. " +
                         "Possible types: {Types}", type.Name, string.Join(", ", Entities.Keys));
        return new Dictionary<int, T>();
    }
}
