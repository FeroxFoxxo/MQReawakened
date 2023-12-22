using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Server.Reawakened.Rooms.Services;

public class SyncEventManager(EventSink sink) : IService
{
    private MethodInfo _decodeSync;
    private MethodInfo _encodeSync;

    private RoomManager _roomManager;

    public void Initialize() => sink.WorldLoad += CreateSyncHandlers;

    public void CreateSyncHandlers()
    {
        _roomManager = (RoomManager)
            RuntimeHelpers.GetUninitializedObject(typeof(RoomManager));

        _encodeSync = _roomManager.GetMethod("EncodeEvent");
        _decodeSync = _roomManager.GetMethod("DecodeEvent");
    }

    public string EncodeEvent(SyncEvent syncEvent) =>
        (string)_encodeSync.Invoke(_roomManager, [syncEvent]);

    public SyncEvent DecodeEvent(string[] fields) =>
        (SyncEvent)_decodeSync.Invoke(_roomManager, [fields]);
}
