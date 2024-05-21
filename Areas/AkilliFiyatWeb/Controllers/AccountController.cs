using System.IO.Pipelines;
using AkilliFiyatWeb.Models;
using AkilliFiyatWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPortfolyoWebSite.Models;

namespace AkilliFiyatWeb.Controllers
{
    [Area("AkilliFiyatWeb")]
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;
        private RoleManager<AppRole> _roleManager;
        private SignInManager<AppUser> _signInManager;
        private IEmailSender _emailSender;
        private MyLogger _log;
        public AccountController(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            MyLogger myLogger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _log = myLogger;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Models.LoginViewModel model)
        {
            try
            {
				if (ModelState.IsValid)
				{
					var user = await _userManager.FindByEmailAsync(model.Email);

					if (user != null)
					{
						await _signInManager.SignOutAsync();


						var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

						if (result.Succeeded)
						{
							await _userManager.ResetAccessFailedCountAsync(user);
							await _userManager.SetLockoutEndDateAsync(user, null);

							return Redirect("/akilli-fiyat/");

						}
						else if (result.IsLockedOut)
						{
							var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
							var timeLeft = lockoutDate.Value - DateTime.UtcNow;
							ModelState.AddModelError("", $"Hesabınız kitlendi, Lütfen {timeLeft.Minutes} dakika sonra deneyiniz");
						}
						else
						{
							ModelState.AddModelError("", "parolanız hatalı");
						}
					}
					else
					{
						ModelState.AddModelError("", "bu email adresiyle bir hesap bulunamadı");
					}
				}
				return View(model);
			}
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new AppUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        FullName = model.FullName
                    };

                    IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var url = Url.Action("ConfirmEmail", "Account", new { id = user.Id, token });

                        // email
                        await _emailSender.SendEmailAsync(user.Email, "Hesap Onayı", $"Lütfen email hesabınızı onaylamak için linke <a href='https://hulusimsek.com/akilli-fiyat{url}'>tıklayınız.</a>");

                        TempData["message"] = "Email hesabınızdaki onay mailini tıklayınız.";
                        return Redirect("/akilli-fiyat/Account/Login");
                    }

                    foreach (IdentityError err in result.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Log("1", ex.Message, ex.ToString());
                return View("Error");
            }


            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            if (id == null)
            {
                TempData["message"] = "Geçersiz id bilgisi";
                return Redirect("/akilli-fiyat/Account/Login");
            }
            if (token == null)
            {
                TempData["message"] = "Geçersiz token bilgisi";
                return Redirect("/akilli-fiyat/Account/Login");
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    TempData["message"] = "Hesabınız onaylandı";
                    return Redirect("/akilli-fiyat/Account/Login");
                }
            }
            else
            {
                TempData["message"] = "Kullanıcı bulunamadı";
                return Redirect("/akilli-fiyat/Account/Login");
            }
            return Redirect("/akilli-fiyat/Account/Login");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/akilli-fiyat/Account/Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["message"] = "Eposta adresinizi giriniz.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                TempData["message"] = "Eposta adresiyle eşleşen bir kayıt yok.";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var url = Url.Action("ResetPassword", "Account", new { user.Id, token });

            await _emailSender.SendEmailAsync(Email, "Parola Sıfırlama", $"Parolanızı yenilemek için linke <a href='https://hulusimsek.com/akilli-fiyat{url}'>tıklayınız.</a>.");

            TempData["message"] = "Eposta adresinize gönderilen link ile şifrenizi sıfırlayabilirsiniz.";

            return View();

        }

    }
}