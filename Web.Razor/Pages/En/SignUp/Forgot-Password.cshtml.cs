using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Base.Core.Services;
using Server.Base.Database.Accounts;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Services;
using System;
using System.ComponentModel.DataAnnotations;
using Web.Razor.Services;

namespace Web.Razor.Pages.En.SignUp;

public class Forgot_PasswordModel(InternalRwConfig iConfig, ServerRwConfig sConfig, AccountHandler aHandler,
    EmailService email, TemporaryDataStorage tempStorage, RandomKeyGenerator keyGenerator) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }
    }

    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (aHandler.ContainsUsername(Input.Username))
        {
            var account = aHandler.GetAccountFromUsername(Input.Username);

            var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(account.Id.ToString());

            tempStorage.AddData(sId, account.Write);

            //await email.SendPasswordResetEmailAsync(account.Email, $"https://{sConfig.DomainName}/en/signup/reset-password?id={sId}");
        }
        else
        {
            var r = new Random();

            var delay = r.Next(50, 150);

            await Task.Delay(delay);
        }

        return RedirectToPage("ResetPasswordConfirmation");
    }

}
