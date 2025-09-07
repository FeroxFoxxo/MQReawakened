using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.ReleaseNotes;

namespace Web.Razor.Pages.En;

public class ReleaseNotesModel : PageModel
{
    private readonly InternalRwConfig _config;
    private readonly InternalReleaseNotes _notesBundle;

    public List<ReleaseNote> ReleaseNotes => _notesBundle.ReleaseNotes ?? new List<ReleaseNote>();

    public ReleaseNotesModel(InternalRwConfig config, InternalReleaseNotes notesBundle)
    {
        _config = config;
        _notesBundle = notesBundle;
    }

    public void OnGet() => ViewData["ServerName"] = _config.ServerName;
}
