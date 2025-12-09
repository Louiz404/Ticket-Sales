using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketSales.Models;
using TicketSales.Services;

namespace TicketSales.Controllers
{
    public class ClienteController : Controller
    {
        private readonly TicketService _service;
        public ClienteController(TicketService service)
        {
            _service = service;
        }

        // GET
        public ActionResult Index()
        {
            var clientes = _service.ListarClientesAtivos();
            return View(clientes);
        }


        // GET:
        public ActionResult Criar()
        {
            return View();
        }

        // POST: 
        [HttpPost]
        public ActionResult Criar(Cliente cliente)
        {
            try
            {
                _service.CadastrarCliente(cliente.Nome, cliente.Email, cliente.Idade);

                TempData["Sucesso"] = "Cliente cadastrado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao cadastrar cliente: {ex.Message}");
                return View(cliente);
            }
        }

        // POST: 
        [HttpPost]
        public ActionResult Desativar(int id)
        {
            try
            {
                _service.DesativarCliente(id);
                TempData["Sucesso"] = "Cliente desativado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao desativar cliente: {ex.Message}";
            }
            
            return RedirectToAction("Index");
            
        }
     
    }
}
