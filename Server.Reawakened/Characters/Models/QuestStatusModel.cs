using A2m.Server;
using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class QuestStatusModel
{
    public const char FieldSeparator = '|';

    public int Id { get; set; }
    public QuestStatus.QuestState QuestStatus { get; set; }
    public Dictionary<int, ObjectiveModel> Objectives { get; set; }

    public QuestStatusModel() =>
        Objectives = new Dictionary<int, ObjectiveModel>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        sb.Append(Id);
        sb.Append((int)QuestStatus);

        foreach (var objective in Objectives)
        {
            sb.Append(objective.Key);
            sb.Append(objective.Value.Completed ? 1 : 0);
            sb.Append(objective.Value.CountLeft);
        }

        return sb.ToString();
    }
}
