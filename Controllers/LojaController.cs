using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        public IActionResult Comprar(int eventoId,List<int> assentosSelecionados, TiposDePagamento metodoPagamento)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = _service.ObterClientePorUsuarioId(userId);

                if (cliente == null) throw new Exception("Seu usuario não tem um perfil de cliente associado");


                _service.RegistrarCompra(cliente.Id, eventoId, assentosSelecionados, metodoPagamento); 
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
