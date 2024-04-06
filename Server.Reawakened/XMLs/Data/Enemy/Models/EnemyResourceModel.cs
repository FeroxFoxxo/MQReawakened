using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.XMLs.Data.Enemy.Models;

public class EnemyResourceModel(string type, string asset)
{
    public string Type => type;
    public string Resource => asset;

    public override string ToString()
    {
        var asset = new SeparatedStringBuilder('-');

        asset.Append(Type);
        asset.Append(Resource);

        return asset.ToString();
    }
}
