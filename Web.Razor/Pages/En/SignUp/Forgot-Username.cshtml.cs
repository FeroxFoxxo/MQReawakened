using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Database.Accounts;
using Server.Reawakened.Network.Services;
using System.ComponentModel.DataAnnotations;
using Web.Razor.Services;

namespace Web.Razor.Pages.En.SignUp;

[BindProperties]
public class Forgot_UsernameModel(InternalRwConfig config, AccountHandler aHandler,
    PagesService email, TemporaryDataStorage tempStorage, RandomKeyGenerator keyGenerator) : PageModel
{
    [Required(ErrorMessage = "Please Enter Email Address")]
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
    public string Email { get; set; }

    public void OnGet() => ViewData["ServerName"] = config.ServerName;

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        Email = Email.Sanitize();

        if (string.IsNullOrEmpty(Email))
            return Page();

        if (aHandler.ContainsEmail(Email))
        {
            var account = aHandler.GetAccountFromEmail(Email);

            var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(account.Id.ToString());

            tempStorage.AddData(sId, account.Write);

            await email.SendUsernameResetEmailAsync(account.Email, $"{config.GetHostAddress()}/en/signup/resetusername?id={sId}", account.Email);
        }
        else
        {
            await PagesService.Delay();
        }

        return RedirectToPage("ForgotUsernameConfirmation");
    }

}
