using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Database.Accounts;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Players.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Web.Razor.Pages.En;

[BindProperties]
public class SignUpModel(AccountHandler accountHandler, UserInfoHandler userInfoHandler, ILogger<SignUpModel> logger, InternalRwConfig config) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; }

    [Required(ErrorMessage = "Please Enter Username")]
    [Display(Name = "User Name")]
    [StringLength(15, ErrorMessage = "The {0} cannot be over {1} characters long.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Please Enter Password")]
    [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Please Enter Confirm Password")]
    [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Please Enter Email Address")]
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Please Enter Gender")]
    public Gender Gender { get; set; }

    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Please Enter Date")]
    public DateTime? Date { get; set; }

    [Required(ErrorMessage = "Please Enter Region")]
    public string Region { get; set; }

    public static List<SelectListItem> Genders => [.. Enum.GetValues<Gender>()
        .Select(v => new SelectListItem
        {
            Text = v.ToString(),
            Value = ((int)v).ToString()
        })];

    public static List<SelectListItem> Regions => [.. CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(ci => new RegionInfo(ci.ToString()))
        .DistinctBy(ci => ci.TwoLetterISORegionName)
        .OrderBy(x => x.EnglishName)
        .Select(x => new SelectListItem
        {
            Text = x.EnglishName +
                   (x.EnglishName == x.NativeName ? string.Empty : $"/{x.NativeName}"),
            Value = x.TwoLetterISORegionName
        })];

    public void OnGet() => ViewData["ServerName"] = config.ServerName;

    public IActionResult OnPost()
    {
        ViewData["ServerName"] = config.ServerName;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!Date.HasValue)
        {
            StatusMessage = "An error occurred while ensuring data value!";
            return Page();
        }

        Username = Username.Sanitize();
        Email = Email.Sanitize();

        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Region))
        {
            return Page();
        }

        if (ConfirmPassword != Password)
        {
            return Page();
        }

        if (accountHandler.ContainsUsername(Username))
        {
            StatusMessage = "An account already exists with this username!";
            return Page();
        }

        if (accountHandler.ContainsEmail(Email))
        {
            StatusMessage = "An account already exists with this email!";
            return Page();
        }

        var ip = Request.HttpContext.Connection.RemoteIpAddress;

        if (ip == null)
        {
            StatusMessage = "A bad request occured. Try on a different device.";
            return Page();
        }

        var account = accountHandler.Create(ip, Username, Password, Email);

        if (account == null)
        {
            logger.LogError("Could not create account with name: {Username}", Username);

            StatusMessage = "Could not create an account! " +
                "You could have too many, or have put strange characters in your username/password. " +
                "Ensure these consist of English characters, if possible.";

            return Page();
        }

        var userInfo = userInfoHandler.Create(ip, account.Id, Gender, Date.Value, Region, "Website");

        if (userInfo == null)
        {
            logger.LogError("Could not create user info with name: {Username}", Username);

            StatusMessage = "Could not create any user information! " +
                "Perhaps an account already exists with this username?";

            return Page();
        }

        return RedirectToPage("Success");
    }
}
