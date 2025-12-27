using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Password;
using MySuperSystem2025.Services.Interfaces;

namespace MySuperSystem2025.Controllers
{
    /// <summary>
    /// Password controller handling secure credential storage functionality
    /// </summary>
    [Authorize]
    public class PasswordController : Controller
    {
        private readonly IPasswordService _passwordService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<PasswordController> _logger;

        public PasswordController(
            IPasswordService passwordService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PasswordController> logger)
        {
            _passwordService = passwordService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: /Password
        public async Task<IActionResult> Index(int? categoryId = null, string? search = null)
        {
            var dashboard = await _passwordService.GetDashboardAsync(UserId, categoryId, search);
            return View(dashboard);
        }

        // GET: /Password/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _passwordService.GetCategoriesAsync(UserId);
            var model = new CreatePasswordViewModel
            {
                Categories = new SelectList(categories, "Id", "Name")
            };
            return View(model);
        }

        // POST: /Password/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _passwordService.GetCategoriesAsync(UserId);
                model.Categories = new SelectList(categories, "Id", "Name");
                return View(model);
            }

            var result = await _passwordService.CreatePasswordAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Password stored securely.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to store password.";
            var cats = await _passwordService.GetCategoriesAsync(UserId);
            model.Categories = new SelectList(cats, "Id", "Name");
            return View(model);
        }

        // GET: /Password/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var password = await _passwordService.GetPasswordForEditAsync(id, UserId);
            if (password == null)
            {
                return NotFound();
            }

            var categories = await _passwordService.GetCategoriesAsync(UserId);
            password.Categories = new SelectList(categories, "Id", "Name");
            return View(password);
        }

        // POST: /Password/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPasswordViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var categories = await _passwordService.GetCategoriesAsync(UserId);
                model.Categories = new SelectList(categories, "Id", "Name");
                return View(model);
            }

            var result = await _passwordService.UpdatePasswordAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Password updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to update password.";
            var cats = await _passwordService.GetCategoriesAsync(UserId);
            model.Categories = new SelectList(cats, "Id", "Name");
            return View(model);
        }

        // POST: /Password/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _passwordService.DeletePasswordAsync(id, UserId);
            if (result)
            {
                TempData["Success"] = "Password deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete password.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Password/Reveal/5
        [HttpGet]
        public IActionResult Reveal(int id)
        {
            return View(new RevealPasswordViewModel { PasswordId = id });
        }

        // POST: /Password/Reveal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reveal(RevealPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Re-authenticate the user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.AccountPassword);
            if (!passwordValid)
            {
                ModelState.AddModelError("AccountPassword", "Incorrect password.");
                return View(model);
            }

            // Password verified, reveal the stored password
            var decryptedPassword = await _passwordService.RevealPasswordAsync(model.PasswordId, UserId);
            if (decryptedPassword == null)
            {
                TempData["Error"] = "Failed to reveal password.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RevealedPassword = decryptedPassword;
            return View("RevealResult", model);
        }

        // GET: /Password/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _passwordService.GetCategoriesAsync(UserId);
            return View(categories);
        }

        // GET: /Password/CreateCategory
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new CreatePasswordCategoryViewModel());
        }

        // POST: /Password/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreatePasswordCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _passwordService.CreateCategoryAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Categories));
            }

            ModelState.AddModelError("Name", "Category name already exists.");
            return View(model);
        }

        // GET: /Password/EditCategory/5
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _passwordService.GetCategoryForEditAsync(id, UserId);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /Password/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, EditPasswordCategoryViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _passwordService.UpdateCategoryAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Categories));
            }

            ModelState.AddModelError("Name", "Category name already exists.");
            return View(model);
        }

        // POST: /Password/DeleteCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _passwordService.DeleteCategoryAsync(id, UserId);
            if (result)
            {
                TempData["Success"] = "Category deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Cannot delete this category. It may be a default category or have passwords associated.";
            }

            return RedirectToAction(nameof(Categories));
        }
    }
}
