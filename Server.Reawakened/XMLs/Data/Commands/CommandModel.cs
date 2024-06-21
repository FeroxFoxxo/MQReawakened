using Server.Base.Accounts.Enums;

namespace Server.Reawakened.XMLs.Data.Commands;
public abstract class CommandModel
{
    public abstract string CommandName { get; }
    public abstract string CommandDescription { get; }
    public abstract List<ParameterModel> Parameters { get; }
    public abstract AccessLevel AccessLevel { get; }
}
