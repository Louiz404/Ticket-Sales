using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketSales.Models;
using TicketSales.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TicketSales.Data;
using TicketSales.Models.ViewModels;
using Rotativa.AspNetCore;

namespace TicketSales.Controllers
{
    public class LojaController : Controller
    {
        private readonly TicketService _service;
        private readonly TicketContext _TicketContext;
        public LojaController(TicketService service, TicketContext tickeContext)
        {
            _service = service;
            _TicketContext = tickeContext;
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

        [Authorize]
        public IActionResult MeusIngressos()
        { 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var compras = _TicketContext.Compras
                .Include(c => c.Evento)
                .Include(c => c.AssentosSelecionados)
                .Where(c => c.Cliente.UsuarioId == userId)
                .OrderByDescending(c => c.DataCompra)
                .ToList();

            return View(compras);
        }

        [Authorize]
        public IActionResult GerarQRCode(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var qrCodeBytes = _service.GerarBytesQRCode(id, userId);

            if (qrCodeBytes == null)
            {
                return NotFound();
            }

            return File(qrCodeBytes, "image/png");

        }

        [Authorize]
        public IActionResult DownloadTicket(int id)
        { 
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = _service.DownloadTicket(id, userId);

            if (model == null) return NotFound();

            return new ViewAsPdf("TicketPdf", model)
            {
                FileName = $"Ingresso_Pedido_{model.Compra.Id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking --margin-top 0mm --margin-bottom 0mm --margin-left 0mm --margin-right 0mm"
            };

        }
    }
}
