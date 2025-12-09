using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;
using TicketSales.Models;
using TicketSales.Services;

namespace TicketSales.Controllers
{
    public class EventoController : Controller
{
        private readonly TicketService _service;
        private readonly IWebHostEnvironment _webHostEnvironment; // Acesssar pastas
        
        
        public EventoController(TicketService service, IWebHostEnvironment webHostEnvironment)
        {
            _service = service;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var eventos = _service.ListarEventosAtivos();
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
                    
                _service.CriarEvento(evento.Nome, evento.QuantidadeLugares, evento.Valor, evento.Categoria, nomeArquivo);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(evento);
            }
        }

        
        [HttpPost]
        public IActionResult Desativar(int id)
        {
            try
            {
                _service.DesativarEvento(id);
            }
            catch (Exception ex) {
                TempData["Erro, não foi possivel concluir a ação"] = ex.Message;
            }
                return RedirectToAction("Index");
        }
    }
}
