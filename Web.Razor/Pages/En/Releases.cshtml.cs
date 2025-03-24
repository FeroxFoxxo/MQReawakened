using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.ReleaseNotes;

namespace Web.Razor.Pages.En;

public class ReleaseNotesModel(InternalRwConfig iConfig, InternalReleaseNotes internalReleaseNotes) : PageModel
{
    // Define a list of release notes (version, release date, and notes)
    public List<ReleaseNote> ReleaseNotes => internalReleaseNotes.ReleaseNotes;

    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;
}
