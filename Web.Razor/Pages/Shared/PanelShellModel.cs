namespace Web.Razor.Pages.Shared;

public class PanelShellModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Kicker { get; set; }
    public bool Center { get; set; }
    public object Actions { get; set; } // allow IHtmlContent via dynamic rendering
    public string Style { get; set; }
    public string AdditionalClasses { get; set; }
    public bool ShowHeader { get; set; } = true;
}