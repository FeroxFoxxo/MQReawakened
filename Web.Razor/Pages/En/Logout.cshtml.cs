using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages.En;

public class LogoutModel(InternalRwConfig config) : PageModel
{
    public IActionResult OnPost()
    {
        // Delete cookies
        Response.Cookies.Delete("UserId", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("Username", new CookieOptions { Path = "/" });

        // Redirect after logout
        return Redirect("/");
    }
}
