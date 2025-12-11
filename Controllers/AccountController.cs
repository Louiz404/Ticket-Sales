using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSales.Services;
using TicketSales.Models;

namespace TicketSales.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TicketService _service;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            TicketService service)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _service = service;
        }
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<ActionResult> Register(string email, string password, string nome, int idade)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                if (email.Contains("org"))
                {
                    await _userManager.AddToRoleAsync(user, "Organizador");

                    return RedirectToAction("Index", "Evento");
                }

                else
                {
                    await _userManager.AddToRoleAsync(user, "Cliente");

                    _service.CadastrarClienteVinculado(nome, email, idade, user.Id);

                    return RedirectToAction("Index", "Loja");

                }        
    
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("Erro", error.Description);
            }
            return View();

        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Evento");
                }

                return RedirectToAction("Index", "Loja");
            }
            ModelState.AddModelError("Erro", "Login inválido.");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Loja");
        }

        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}

