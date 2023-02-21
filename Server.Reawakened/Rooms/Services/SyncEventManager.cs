using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Server.Reawakened.Rooms.Services;

public class SyncEventManager : IService
{
    private readonly EventSink _sink;
    private MethodInfo _decodeSync;
    private MethodInfo _encodeSync;

    private RoomManager _roomManager;

    public SyncEventManager(EventSink sink) => _sink = sink;

    public void Initialize() => _sink.WorldLoad += CreateSyncHandlers;

    public void CreateSyncHandlers()
    {
        _roomManager = (RoomManager)
            FormatterServices.GetUninitializedObject(typeof(RoomManager));

        _encodeSync = _roomManager.GetMethod("EncodeEvent");
        _decodeSync = _roomManager.GetMethod("DecodeEvent");
    }

    public string EncodeEvent(SyncEvent syncEvent) =>
        (string)_encodeSync.Invoke(_roomManager, new object[] { syncEvent });

    public SyncEvent DecodeEvent(string[] fields) =>
        (SyncEvent)_decodeSync.Invoke(_roomManager, new object[] { fields });
}
