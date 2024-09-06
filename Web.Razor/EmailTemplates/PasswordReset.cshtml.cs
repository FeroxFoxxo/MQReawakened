using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.EmailTemplates;

public class PasswordResetModel(InternalRwConfig iConfig) : PageModel
{
    public string Username { get; set; }
    public string ResetLink { get; set; }

    public string Domain => iConfig.GetHostAddress();
    public string SiteName => iConfig.ServerName;
}
