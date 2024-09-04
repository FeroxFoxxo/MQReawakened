using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Base.Core.Services;
using Server.Base.Database.Accounts;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Services;
using System.ComponentModel.DataAnnotations;
using Web.Razor.Services;

namespace Web.Razor.Pages.En.SignUp;

public class Forgot_UsernameModel(InternalRwConfig iConfig, ServerRwConfig sConfig, AccountHandler aHandler,
    EmailService email, TemporaryDataStorage tempStorage, RandomKeyGenerator keyGenerator) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (aHandler.ContainsEmail(Input.Email))
        {
            var account = aHandler.GetAccountFromEmail(Input.Email);

            var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(account.Id.ToString());

            tempStorage.AddData(sId, account.Write);

            //await email.SendUsernameResetEmailAsync(account.Email, $"https://{sConfig.DomainName}/en/signup/reset-username?id={sId}");
        }
        else
        {
            var r = new Random();

            var delay = r.Next(50, 150);

            await Task.Delay(delay);
        }

        return RedirectToPage("ResetUsernameConfirmation");
    }

}
