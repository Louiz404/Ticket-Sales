using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSales.Data;
using TicketSales.Models;

namespace TicketSales.Controllers
{
    [Authorize(Roles = "Admin")] // Só Admin acessa esse controller
    public class ClienteController : Controller
    {
        private readonly TicketContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ClienteController(TicketContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var clientes = _context.Clientes.OrderBy(c => c.Nome).ToList();
            return View(clientes);
        }

        // === EDITAR (GET) ===
        public IActionResult Editar(int id)
        {
            var cliente = _context.Clientes.Find(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // === EDITAR (POST) ===
        [HttpPost]
        public IActionResult Editar(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Clientes.Update(cliente);
                _context.SaveChanges();
                TempData["Sucesso"] = "Dados do cliente atualizados com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // === DESATIVAR ===
        [HttpPost]
        public IActionResult Desativar(int id)
        {
            var cliente = _context.Clientes.Find(id);
            if (cliente != null)
            {
                cliente.Ativo = false;
                _context.SaveChanges();
                TempData["Sucesso"] = "Cliente desativado com sucesso.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ResetarSenha(int id)
        {
            var cliente = _context.Clientes.Find(id);
            if (cliente == null) return NotFound();

            // 1. Acha o usuário de login pelo ID vinculado
            var user = await _userManager.FindByIdAsync(cliente.UsuarioId);

            if (user != null)
            {
                // 2. Gera um token de reset (permissão para trocar senha)
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // 3. Força a nova senha padrão
                var resultado = await _userManager.ResetPasswordAsync(user, token, "Ticket@2025");

                if (resultado.Succeeded)
                {
                    TempData["Sucesso"] = $"Senha de {cliente.Nome} resetada para 'Ticket@2025'.";
                }
                else
                {
                    TempData["Erro"] = "Erro ao resetar senha no sistema de login.";
                }
            }
            else
            {
                TempData["Erro"] = "Usuário de login não encontrado.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}