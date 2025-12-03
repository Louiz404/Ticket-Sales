using Microsoft.EntityFrameworkCore;
using TicketSales.Data;
using TicketSales.Models;

namespace TicketSales.Services
{
    public class TicketService
    {
        private readonly TicketContext _ticketContext;

        public TicketService(TicketContext context)
        {
            _ticketContext = context;
        }

        public void RegistrarCompra(int clinteId, int eventoId, List<int> assentosIds, TiposDePagamento metodoPagamento)
        {
            var cliente = _ticketContext.Clientes.Find(clinteId);

            var evento = _ticketContext.Eventos
                .Include(e => e.Assentos)
                .FirstOrDefault(e => e.Id == eventoId);

            if (cliente == null || !cliente.Ativo)
            {
                throw new Exception("Cliente inválido ou inativo");
            }

            if (evento == null || !evento.Ativo)
            {
                throw new Exception("Evento inválido ou inativo");
            }

            // Validar Assentos
            var assentosSelecionados = evento.Assentos
                .Where(a => assentosIds.Contains(a.Id) && !a.Ocupado)
                .ToList();

            var compra = new Compra
            {
                Cliente = cliente,
                Evento = evento,
                AssentosSelecionados = assentosSelecionados,
                MetodoPagamento = metodoPagamento,
                DataCompra = DateTime.Now
            };

            _ticketContext.Compras.Add(compra);
            _ticketContext.SaveChangesAsync();
        }

        //Adionar outros métodos (listar, Cancelar) etc..

        public void CancelarCompra(int compraId)
        {
            var compra = _ticketContext.Compras.FirstOrDefault(c => c.Id == compraId);

            if (compra == null)
                throw new Exception("Compra não encontrada.");

            foreach (var assento in compra.AssentosSelecionados)
            {
                assento.Ocupado = false;
            }
            compra.Evento.LugaresDisponiveis += compra.AssentosSelecionados.Count;

            _ticketContext.Compras.Remove(compra);
            Console.WriteLine($"Compra {compraId} cancelada com sucesso, e assentos liberados.");
        }

        public List<Compra> ListarTodasCompras()
        {
            return _ticketContext.Compras
                .Include(c => c.Cliente)
                .Include(c => c.Evento)
                .Include(c => c.AssentosSelecionados)
                .ToList();
        }

        public List<Compra> ListarComprasPorEvento(int eventoId)
        {
            return _ticketContext.Compras.Where(c => c.Evento.Id == eventoId).ToList();

        }
    }
}
