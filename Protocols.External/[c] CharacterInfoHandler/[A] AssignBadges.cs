using A2m.Server;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;

namespace Protocols.External._c__CharacterInfoHandler
{
    public class AssignBadges : ExternalProtocol
    {
        public override string ProtocolName => "cA";

        public override void Run(string[] message)
        {
            var tribeDataList = new List<Dictionary<TribeType, TribeDataModel>>();
            var tribeProgressionData = new Dictionary<TribeType, TribeDataModel>();

            foreach (var tribe in message.Skip(5))
            {
                if (string.IsNullOrEmpty(tribe)) continue;

                var tribeData = tribe.Split('|');
                var tribeType = (TribeType)Enum.Parse(typeof(TribeType), tribeData[0]);
                var pointAmount = int.Parse(tribeData[1]);
                var unlocked = tribeData[2] == "1";

                var tribeTypeModel = new TribeDataModel()
                {
                    TribeType = tribeType,
                    BadgePoints = pointAmount,
                    Unlocked = unlocked
                };
                tribeProgressionData.Add(tribeType, tribeTypeModel);

                tribeDataList.Add(tribeProgressionData);
            }

            var autoAssign = 0;
            if (Player.Character.Data.TribesProgression.Values.Count % 5 == 0)
                autoAssign = 1;

            Player.Character.Data.TribesProgression = tribeProgressionData;

            SendXt("cA", GenerateTribeData(tribeProgressionData),
                (int)Player.Character.Data.Allegiance, autoAssign);
        }

        private string GenerateTribeData(Dictionary<TribeType, TribeDataModel> dataList)
        {
            var sb = new SeparatedStringBuilder('<');

            foreach (var tribeData in dataList)
                sb.Append($"{(int)tribeData.Key}|{tribeData.Value.BadgePoints}|{(tribeData.Value.Unlocked ? 1 : 0)}");

            return sb.ToString();
        }
    }
}
