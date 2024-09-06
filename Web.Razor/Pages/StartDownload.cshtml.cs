using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Razor.Services;

namespace Web.Razor.Pages;

public class StartDownloadModel(PagesService pService) : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        var memory = new MemoryStream();

        using (var stream = new FileStream(pService.ZipPath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }

        memory.Position = 0;

        var fileName = Path.GetFileName(pService.ZipPath);

        return File(memory, "application/octet-stream", fileName);
    }
}
