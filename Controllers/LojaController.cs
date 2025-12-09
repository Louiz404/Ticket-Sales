using Microsoft.AspNetCore.Mvc;
using TicketSales.Models;
using TicketSales.Services;

namespace TicketSales.Controllers
{
    public class LojaController : Controller
    {
        private readonly TicketService _service;
        public LojaController(TicketService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            var eventos = _service.ListarEventosAtivos();
            return View(eventos);
        }

        public IActionResult Detalhes(int id)
        {
            var evento = _service.ObterEventoPorId(id);
            if (evento == null) return NotFound();

            return View(evento);
        }

        [HttpPost]
        public IActionResult Comprar(int eventoId, int clienteId, List<int> assentosSelecionados, TiposDePagamento metodoPagamento)
        {
            try
            {
                _service.RegistrarCompra(clienteId, eventoId, assentosSelecionados, metodoPagamento);
                return RedirectToAction("Sucesso");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Detalhes", new { id = eventoId });
            }
        }

            public IActionResult Sucesso()
            {
                return View();
        }
    }
}
