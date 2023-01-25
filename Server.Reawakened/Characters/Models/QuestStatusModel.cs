using A2m.Server;
using System.Globalization;
using System.Text;

namespace Server.Reawakened.Characters.Models;

public class QuestStatusModel
{
    public const char FieldSeparator = '|';

    public int Id { get; set; }
    public QuestStatus.QuestState QuestStatus { get; set; }
    public Dictionary<int, ObjectiveModel> Objectives { get; set; }

    public QuestStatusModel() {}

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Id.ToString(CultureInfo.InvariantCulture));
        sb.Append(FieldSeparator);
        sb.Append((int)QuestStatus);
        sb.Append(FieldSeparator);

        foreach (var objective in Objectives)
        {
            sb.Append(objective.Key);
            sb.Append(FieldSeparator);
            sb.Append(objective.Value.Completed ? "1" : "0");
            sb.Append(FieldSeparator);
            sb.Append(objective.Value.CountLeft);
            sb.Append(FieldSeparator);
        }

        return sb.ToString();
    }
}
