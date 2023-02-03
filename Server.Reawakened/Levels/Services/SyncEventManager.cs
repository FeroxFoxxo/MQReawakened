using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using System.Reflection;
using System.Runtime.Serialization;

namespace Server.Reawakened.Levels.Services;

public class SyncEventManager : IService
{
    private readonly EventSink _sink;

    private RoomManager _roomManager;
    private MethodInfo _encodeSync;
    private MethodInfo _decodeSync;

    public SyncEventManager(EventSink sink) => _sink = sink;

    public void Initialize() => _sink.WorldLoad += CreateSyncHandlers;

    public void CreateSyncHandlers()
    {
        _roomManager = (RoomManager)
            FormatterServices.GetUninitializedObject(typeof(RoomManager));

        _encodeSync = _roomManager.GetPrivateMethod("EncodeEvent");
        _decodeSync = _roomManager.GetPrivateMethod("DecodeEvent");
    }

    public string EncodeEvent(SyncEvent syncEvent) =>
        (string)_encodeSync.Invoke(_roomManager, new object[] { syncEvent });

    public SyncEvent DecodeEvent(string[] fields) =>
        (SyncEvent)_decodeSync.Invoke(_roomManager, new object[] { fields });
}
