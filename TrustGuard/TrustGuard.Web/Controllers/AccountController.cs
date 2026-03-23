using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrustGuard.Application.Interfaces;
using TrustGuard.Domain.Entities;
using TrustGuard.Web.Models;

namespace TrustGuard.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // --- РОЗУМНИЙ РОУТЕР (Перенаправляє по куках) ---
        [HttpGet]
        public IActionResult Entry()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (Request.Cookies.ContainsKey("HasAccount"))
            {
                return RedirectToAction("Login");
            }

            return RedirectToAction("Register");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);

                    string emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 8px rgba(0,0,0,0.1);'>
                        <div style='background-color: #212529; padding: 20px; text-align: center;'>
                            <h2 style='color: #ffffff; margin: 0; letter-spacing: 1px;'>TrustGuard</h2>
                        </div>
                        <div style='padding: 30px; background-color: #ffffff; text-align: center;'>
                            <h3 style='color: #333; margin-top: 0;'>Вітаємо, {model.FullName}!</h3>
                            <p style='color: #555; font-size: 16px; line-height: 1.5;'>Дякуємо за реєстрацію. Щоб завершити створення акаунту та почати роботу, будь ласка, підтвердіть свою електронну пошту, натиснувши на кнопку нижче:</p>
                            <a href='{confirmationLink}' style='display: inline-block; margin-top: 20px; padding: 12px 30px; background-color: #198754; color: #ffffff; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>Підтвердити акаунт</a>
                        </div>
                    </div>";

                    try
                    {
                        await _emailSender.SendEmailAsync(model.Email, "Підтвердження реєстрації TrustGuard", emailBody);

                        // ДОДАЄМО КУКУ НА 1 РІК
                        Response.Cookies.Append("HasAccount", "true", new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1),
                            HttpOnly = true
                        });

                        TempData["SuccessMessage"] = "Реєстрація успішна! На вашу пошту відправлено лист для підтвердження.";
                        return RedirectToAction("Login", "Account");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, $"Помилка відправки листа: {ex.Message}");
                        await _userManager.DeleteAsync(user);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Користувача не знайдено.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Вашу пошту успішно підтверджено! Тепер ви можете увійти.";
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = "Помилка підтвердження пошти. Можливо, посилання застаріло.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, "УВАГА: Спочатку підтвердіть вашу пошту! Перевірте скриньку (та папку Спам).");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        // ОНОВЛЮЄМО КУКУ ПРИ ВХОДІ
                        Response.Cookies.Append("HasAccount", "true", new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1),
                            HttpOnly = true
                        });

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Невірний логін або пароль");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}