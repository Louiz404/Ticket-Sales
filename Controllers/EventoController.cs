using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Threading.Tasks;
using TicketSales.Models;
using TicketSales.Services;

namespace TicketSales.Controllers
{
    [Authorize(Roles = "Admin,Organizador")]
    public class EventoController : Controller
    {
        private readonly TicketService _service;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;


        public EventoController(
            TicketService service,
            IWebHostEnvironment webHostEnvironment,
            UserManager<IdentityUser> userManager
        )
        {
            _service = service;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var eventos = _service.ListarEventosParaGerenciamento(userId, isAdmin);
            return View(eventos);
        }

        // GET
        public IActionResult Criar()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Criar(Evento evento, IFormFile? foto)
        {
            try
            {
                string? nomeArquivo = null;

                if (foto != null && foto.Length > 0)
                {
                    // 1. Define onde salvar (wwwroot/imagens)
                    string pastaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "imagens");

                    // Cria a pasta se não existir
                    if (!Directory.Exists(pastaDestino)) Directory.CreateDirectory(pastaDestino);

                    // Gera um nome unico para o arquivo
                    string nomeUnico = Guid.NewGuid().ToString() + "_" + foto.FileName;
                    nomeArquivo = nomeUnico; // Guardado para o banco


                    string caminhoCompleto = Path.Combine(pastaDestino, nomeUnico);

                    // Salva o carquivo fisicamente
                    using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _service.CriarEvento(
                    evento.Nome,
                    evento.QuantidadeLugares,
                    evento.Valor,
                    evento.Categoria,
                    nomeArquivo,
                    userId,
                    evento.Local,
                    evento.Endereco,
                    (DateTime)evento.DataEvento,
                    evento.Latitude,
                    evento.Longitude
                    );

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(evento);
            }
        }

        public IActionResult Editar(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                var evento = _service.ObterEventoParaEdicao(id, userId, isAdmin);

                return View(evento);
            }

            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Editar(int id, Evento evento, IFormFile? foto)
        {
            try
            {
                string? nomeArquivo = null;

                if (foto != null && foto.Length > 0)
                {
                    string pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens");

                    if (Directory.Exists(pasta)) Directory.CreateDirectory(pasta);

                    nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    string caminhoCompleto = Path.Combine(pasta, nomeArquivo);

                    using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                _service.AtualizarEvento(id, evento, nomeArquivo, userId, isAdmin);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(evento);
            }
        }

        [HttpPost]
        public IActionResult Desativar(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                _service.DesativarEvento(id, userId, isAdmin);
            }
            catch (Exception ex)
            {
                TempData["Erro, não foi possivel concluir a ação"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ativar(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole("Admin");

                _service.AtivarEvento(id, isAdmin, userId);

                TempData["Sucesso"] = "Evento reativado com sucesso! Ele voltou para a loja.";

                return RedirectToAction(nameof(Editar), new { id = id });
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao ativar: {ex.Message}";
                return RedirectToAction(nameof(Editar), new { id = id });

            }
        }
    }
}
