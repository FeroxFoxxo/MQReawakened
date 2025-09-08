namespace Web.Razor.Pages.Shared;

public class PanelShellModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Kicker { get; set; }
    public object Actions { get; set; } 
    public string Style { get; set; }
    public string AdditionalClasses { get; set; }
    public bool ShowHeader { get; set; } = true;
    public bool Reveal { get; set; } = true;
    public bool CenterHeader { get; set; } = false;
}