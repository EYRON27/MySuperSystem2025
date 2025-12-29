using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Account;
using MySuperSystem2025.Services.Interfaces;

namespace MySuperSystem2025.Controllers
{
    /// <summary>
    /// Account controller handling authentication (login, register, logout)
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IExpenseService _expenseService;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IExpenseService expenseService,
            IPasswordService passwordService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _expenseService = expenseService;
            _passwordService = passwordService;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} logged in successfully", model.Email);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account locked out", model.Email);
                ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username is unique
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                ModelState.AddModelError("UserName", "Username is already taken.");
                return View(model);
            }

            // Check if email is unique
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true // For simplicity, auto-confirm
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add to User role
                await _userManager.AddToRoleAsync(user, "User");

                // Seed default categories
                await _expenseService.SeedDefaultCategoriesAsync(user.Id);
                await _passwordService.SeedDefaultCategoriesAsync(user.Id);

                _logger.LogInformation("User {Email} registered successfully", model.Email);

                // Auto sign in after registration
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/Settings
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new SettingsViewModel
            {
                Profile = new ProfileSettingsViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                }
            };

            return View(model);
        }

        // POST: /Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> UpdateProfile(ProfileSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var settings = new SettingsViewModel { Profile = model };
                return View("Settings", settings);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Check if username is changed and unique
            if (user.UserName != model.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Profile.UserName", "Username is already taken.");
                    var settings = new SettingsViewModel { Profile = model };
                    return View("Settings", settings);
                }
                user.UserName = model.UserName;
            }

            // Check if email is changed and unique
            if (user.Email != model.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Profile.Email", "Email is already registered.");
                    var settings = new SettingsViewModel { Profile = model };
                    return View("Settings", settings);
                }
                user.Email = model.Email;
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully.";
                _logger.LogInformation("User {Email} updated profile", user.Email);
                return RedirectToAction("Settings");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var settingsModel = new SettingsViewModel { Profile = model };
            return View("Settings", settingsModel);
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var settings = new SettingsViewModel
                {
                    Profile = new ProfileSettingsViewModel
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    },
                    ChangePassword = model
                };
                return View("Settings", settings);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(currentUser);
                TempData["Success"] = "Password changed successfully.";
                _logger.LogInformation("User {Email} changed password", currentUser.Email);
                return RedirectToAction("Settings");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("ChangePassword." + (error.Code.Contains("Password") ? "CurrentPassword" : string.Empty), error.Description);
            }

            var settingsViewModel = new SettingsViewModel
            {
                Profile = new ProfileSettingsViewModel
                {
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    UserName = currentUser.UserName ?? string.Empty,
                    Email = currentUser.Email ?? string.Empty,
                    CreatedAt = currentUser.CreatedAt,
                    LastLoginAt = currentUser.LastLoginAt
                },
                ChangePassword = model
            };
            return View("Settings", settingsViewModel);
        }
    }
}
