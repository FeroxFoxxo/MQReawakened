using A2m.Server;
using Newtonsoft.Json.Linq;
using Server.Base;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Protocols.External._c__CharacterInfoHandler
{
    public class AssignBadges : ExternalProtocol
    {
        public override string ProtocolName => "cA";

        public Dictionary<TribeType, TribeDataModel> tribesProgression;

        public override void Run(string[] message)
        {
            var tribeDataList = new List<Dictionary<TribeType, TribeDataModel>>();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--===========--");
            foreach (var msg in message)
                Console.WriteLine(msg);
            Console.WriteLine("--===========--");

            var charData = Player.Character.Data;

            foreach (var tribes in message.Skip(5))
            {
                var tribeData = new TribeDataModel(tribes);

                string GenerateTribeData(Dictionary<TribeType, TribeDataModel> dataList)
                {
                    var sb = new SeparatedStringBuilder('<');

                    foreach (var tribes in dataList)
                        sb.Append(tribes);

                    tribeDataList.Add(dataList);
                    return sb.ToString();
                }

                SendXt("cA", tribeDataList, (int)charData.Allegiance, 1);

            }

        }
    }
}
