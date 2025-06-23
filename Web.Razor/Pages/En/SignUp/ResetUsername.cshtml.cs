using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Accounts.Helpers;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Database.Accounts;
using System.ComponentModel.DataAnnotations;
using Web.Razor.Services;

namespace Web.Razor.Pages.En.SignUp;

[BindProperties]
public class ResetUsernameModel(InternalRwConfig iConfig, AccountHandler aHandler,
    TemporaryDataStorage tempStorage) : PageModel
{
    [Required(ErrorMessage = "Please Enter Username")]
    [Display(Name = "User Name")]
    [StringLength(15, ErrorMessage = "The {0} cannot be over {1} characters long.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Please Confirm Username")]
    [Display(Name = "Confirm User Name")]
    [StringLength(15, ErrorMessage = "The {0} cannot be over {1} characters long.")]
    [Compare("Username", ErrorMessage = "The username and confirmation username do not match.")]
    public string ConfirmUsername { get; set; }

    [Required(ErrorMessage = "Please Enter Current Password")]
    [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string Password { get; set; }

    public async Task<IActionResult> OnGet(string id)
    {
        var account = tempStorage.GetData<AccountDbEntry>(id);

        if (account == null)
        {
            await PagesService.Delay();
            return RedirectToPage("ResetUsernameInvalid");
        }

        ViewData["ServerName"] = iConfig.ServerName;

        return Page();
    }

    public async Task<IActionResult> OnPost(string id)
    {
        if (!ModelState.IsValid)
            return Page();

        var account = tempStorage.GetData<AccountDbEntry>(id);

        if (account == null)
        {
            await PagesService.Delay();
            return RedirectToPage("ResetUsernameInvalid");
        }

        ConfirmUsername = ConfirmUsername.Sanitize();
        Username = Username.Sanitize();

        if (string.IsNullOrEmpty(Username))
        {
            return Page();
        }

        if (ConfirmUsername != Username)
        {
            return Page();
        }

        if (aHandler.ContainsUsername(Username))
        {
            ModelState.AddModelError("Username", "Username already exists");
            return Page();
        }

        var newAccount = aHandler.GetAccountFromEmail(account.Email);

        if (!PasswordHasher.CheckPassword(newAccount, Password))
        {
            ModelState.AddModelError("Password", "Incorrect Password");
            return Page();
        }

        newAccount.Write.Username = Username;

        aHandler.Update(newAccount.Write);

        tempStorage.RemoveData(id, account);

        return RedirectToPage("ResetUsernameSuccessful");
    }

}
