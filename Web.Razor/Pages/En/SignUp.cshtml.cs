using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Services;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Web.Razor.Pages.En;

[BindProperties]
public class SignUpModel(AccountHandler accountHandler, UserInfoHandler userInfoHandler, ILogger<SignUpModel> logger) : PageModel
{
    [Display(Name = "User Name")]
    [StringLength(10, ErrorMessage = "The {0} cannot be over {1} characters long.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Please Enter Password")]
    [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Please Enter Confirm Password")]
    [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
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

    public static List<SelectListItem> Genders => Enum.GetValues<Gender>()
        .Select(v => new SelectListItem
        {
            Text = v.ToString(),
            Value = ((int)v).ToString()
        }).ToList();

    public static List<SelectListItem> Regions => CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(ci => new RegionInfo(ci.ToString()))
        .DistinctBy(ci => ci.TwoLetterISORegionName)
        .OrderBy(x => x.EnglishName)
        .Select(x => new SelectListItem
        {
            Text = x.EnglishName +
                   (x.EnglishName == x.NativeName ? string.Empty : $"/{x.NativeName}"),
            Value = x.TwoLetterISORegionName
        })
        .ToList();

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (!Date.HasValue)
            return Page();

        if (accountHandler.Data.Any(a => a.Value.Username == Username))
            return Forbid();

        if (accountHandler.Data.Any(a => a.Value.Email == Email))
            return Forbid();

        var ip = Request.HttpContext.Connection.RemoteIpAddress;

        if (ip == null || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) ||
            string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Region))
            return BadRequest();

        var account = accountHandler.Create(ip, Username, Password, Email);

        if (account == null)
        {
            logger.LogError("Could not create account with name: {Username}", Username);
            return NotFound();
        }

        var userInfo = userInfoHandler.Create(ip, account.Id, Gender, Date.Value, Region, "Website");

        if (account == null)
        {
            logger.LogError("Could not create user info with name: {Username}", Username);
            return NotFound();
        }

        return RedirectToPage("Success");
    }

}
