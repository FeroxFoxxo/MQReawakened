﻿using Server.Base.Network;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Network.Services;
using System.Xml;

namespace Protocols.System._xml__System;

public class GetRandomKey : SystemProtocol
{
    public override string ProtocolName => "rndK";

    public RandomKeyGenerator KeyGenerator { get; set; }

    public override void Run(XmlDocument xmlDoc) =>
        SendXml("rndK", $"<k>{KeyGenerator.GetRandomKey<NetState>(NetState.ToString())}</k>");
}
