using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TicketSales.Models;
using TicketSales.Services;

namespace TicketSales.Controllers
{
    public class EventoController : Controller
{
        private readonly TicketService _service;
        public EventoController(TicketService service)
        {
            _service = service;
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
        public IActionResult Criar(Evento evento)
        {
            try
            {
                _service.CriarEvento(evento.Nome, evento.QuantidadeLugares, evento.Valor, evento.Categoria);
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
                _service.DesativarCliente(id);
            }
            catch (Exception ex) {
                TempData["Erro, não foi possivel concluir a ação"] = ex;
            }
                return RedirectToAction("Index");
        }
    }
}
