using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Database.Accounts;
using System;
using System.ComponentModel.DataAnnotations;

namespace Web.Razor.Pages.En;

[BindProperties]
public class LoginModel(AccountHandler accountHandler, ILogger<LoginModel> logger, InternalRwConfig config) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; }

    [Required(ErrorMessage = "Please enter your username")]
    [Display(Name = "Username")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Please enter your password")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    public void OnGet() => ViewData["ServerName"] = config.ServerName;

    public IActionResult OnPost()
    {
        ViewData["ServerName"] = config.ServerName;

        if (!ModelState.IsValid)
            return Page();

        Username = Username.Sanitize();

        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            StatusMessage = "Please provide both username and password.";
            return Page();
        }

        var account = accountHandler.GetAccountFromUsername(Username);

        if (account == null || !accountHandler.ValidatePassword(account, Password))
        {
            StatusMessage = "Invalid username or password.";
            return Page();
        }

        // Store cookies
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7), // Expires in 7 days
            Path = "/",
            SameSite = SameSiteMode.Lax
        };

        Response.Cookies.Append("Username", account.Username, cookieOptions);
        Response.Cookies.Append("UserId", account.Id.ToString(), cookieOptions);


        // Redirect to homepage/dashboard
        return Redirect("/");
    }
}
