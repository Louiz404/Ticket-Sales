using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSales.Models;
using TicketSales.Models.ViewModels;
using TicketSales.Services;

namespace TicketSales.Controllers
{
    public class HomeController : Controller
    {
        private readonly TicketService _service;
        
        public HomeController(TicketService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("LandingPage");
            }

            if (User.IsInRole("Cliente"))
            {
                return RedirectToAction("index", "Loja");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            var viewModel = _service.ObterDadosDashboard(userId, isAdmin);

            ViewBag.IsAdmin = isAdmin;

            return View("Dashboard", viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
