using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly DashboardService _dashboard;

        public HomeController(DashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        public async Task<IActionResult> Index()
        {
            var vm = await _dashboard.BuildAsync(DateTime.Now);
            return View(vm);
        }
    }
}
