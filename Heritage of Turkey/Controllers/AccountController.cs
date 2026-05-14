using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Heritage_of_Turkey.Controllers
{
    public class AccountController : Controller
    {
        private const string AdminRole = "Admin";
        private const string DefaultUserRole = "User";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(new RegisterViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            model.ReturnUrl = returnUrl ?? model.ReturnUrl;
            ViewData["ReturnUrl"] = model.ReturnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedDate = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await EnsureDefaultUserRoleExistsAsync();
                await _userManager.AddToRoleAsync(user, DefaultUserRole);
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = "Your account has been created successfully.";

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
                    && Url.IsLocalUrl(model.ReturnUrl)
                    && !IsAdminReturnUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            model.ReturnUrl = returnUrl ?? model.ReturnUrl;
            ViewBag.ReturnUrl = model.ReturnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "You have logged in successfully.";

                var user = await _userManager.FindByEmailAsync(model.Email);
                var isAdmin = user != null && await _userManager.IsInRoleAsync(user, AdminRole);

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
                    && Url.IsLocalUrl(model.ReturnUrl)
                    && (!IsAdminReturnUrl(model.ReturnUrl) || isAdmin))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account has been locked temporarily. Please try again later.");
                return View(model);
            }

            if (result.RequiresTwoFactor)
            {
                ModelState.AddModelError(string.Empty, "Two-factor authentication is required for this account.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have logged out successfully.";

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task EnsureDefaultUserRoleExistsAsync()
        {
            if (!await _roleManager.RoleExistsAsync(DefaultUserRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(DefaultUserRole));
            }
        }

        private static bool IsAdminReturnUrl(string returnUrl)
        {
            return returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
                || returnUrl.StartsWith("~/Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
