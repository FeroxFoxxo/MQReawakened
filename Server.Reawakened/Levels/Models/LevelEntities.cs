using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Network.Protocols;
using System.Reflection;
using System.Runtime.Serialization;

namespace Server.Reawakened.Levels.Models;

public class LevelEntities
{
    public readonly Dictionary<int, List<BaseSyncedEntity>> Entities;

    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public LevelEntities(Level level, LevelHandler handler, ReflectionUtils reflectionUtils,
        IServiceProvider serviceProvider, Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger;

        if (level.LevelPlaneHandler.Planes == null)
            return;

        Entities = new Dictionary<int, List<BaseSyncedEntity>>();

        var invalidProcessable = new List<string>();

        var syncedEntities = typeof(BaseSyncedEntity).Assembly.GetServices<BaseSyncedEntity>()
            .Where(t => t.BaseType != null)
            .Where(t => t.BaseType.GenericTypeArguments.Length > 0)
            .ToDictionary(t => t.BaseType.GenericTypeArguments.First().FullName, t => t);
        
        foreach (var plane in level.LevelPlaneHandler.Planes)
            foreach (var entity in plane.Value.GameObjects)
                foreach (var component in entity.Value.ObjectInfo.Components)
                {
                    if (!handler.ProcessableData.TryGetValue(component.Key, out var mqType))
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
                                if (field.FieldType == typeof(string))
                                    field.SetValue(dataObj, componentValue.Value);
                                else if (field.FieldType == typeof(int))
                                    field.SetValue(dataObj, int.Parse(componentValue.Value));
                                else if (field.FieldType == typeof(bool))
                                    field.SetValue(dataObj, componentValue.Value.ToLower() == "true");
                                else if (field.FieldType == typeof(float))
                                    field.SetValue(dataObj, float.Parse(componentValue.Value));
                                else
                                    _logger.LogError("It is unknown how to convert a string to a {FieldType}. " +
                                                     "Please implement this in the {CurrentType} class.", field.FieldType, GetType().Name);
                            else
                                _logger.LogWarning("Component {ComponentType} does not have field {FieldName}. Possible fields: {Fields}",
                                    mqType, componentValue.Key, string.Join(", ", fields.Select(f => f.Name)));
                        }

                        var storedData = new StoredEntityModel(entity.Value, level, logger);

                        var instancedEntity =
                            reflectionUtils.CreateBuilder<BaseSyncedEntity>(internalType.GetTypeInfo()).Invoke(serviceProvider);

                        var methods = internalType.GetMethods().Where(m =>
                        {
                            var parameters = m.GetParameters();

                            return parameters.Length == 2 &&
                                   parameters[0].ParameterType == dataObj.GetType() &&
                                   parameters[1].ParameterType == storedData.GetType();
                        }).ToList();

                        if (methods.Count != 1)
                            _logger.LogError("Found invalid {Count} amount of initialization methods for {EntityId} ({EntityType})",
                                methods.Count, entity.Key, internalType.Name);
                        else
                            methods.First().Invoke(instancedEntity, new [] { dataObj, storedData });

                        if (!Entities.ContainsKey(entity.Key))
                            Entities.Add(entity.Key, new List<BaseSyncedEntity>());

                        Entities[entity.Key].Add(instancedEntity);
                    }
                    else if (!invalidProcessable.Contains(mqType.Name))
                    {
                        invalidProcessable.Add(mqType.Name);
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
            return entities.ToDictionary(x => x.Id, x => x as T);

        _logger.LogError("Could not find entity with type {TypeName}. Returning empty. " +
                         "Possible types: {Types}", type.Name, string.Join(", ", Entities.Keys));
        return new Dictionary<int, T>();
    }
}
