using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MySuperSystem2025.Controllers
{
    /// <summary>
    /// Main dashboard controller - entry point after login
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        // GET: /Dashboard
        public IActionResult Index()
        {
            return View();
        }
    }
}
