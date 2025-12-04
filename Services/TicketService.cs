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

        // --- MÉTODOS DE COMPRA ---

        public List<Compra> ListarTodasCompras()
        {
            return _ticketContext.Compras
                .Include(c => c.Cliente)
                .Include(c => c.Evento)
                .Include(c => c.AssentosSelecionados).ToList();
        }


        public List<Compra> ListarComprasPorEvento(int eventoId)
        {
            return _ticketContext.Compras
                .Include(c => c.Cliente) // boa pratica incluir cliente
                .Where(c => c.Evento.Id == eventoId).ToList();
        }


        // Métodos de Compra: POST

        public void RegistrarCompra(int clinteId, int eventoId, List<int> assentosIds, TiposDePagamento metodoPagamento)
        {
            var cliente = _ticketContext.Clientes.Find(clinteId);

            var evento = _ticketContext.Eventos
                .Include(e => e.Assentos)
                .FirstOrDefault(e => e.Id == eventoId);

            if (cliente == null || !cliente.Ativo) throw new Exception("Cliente inválido ou inativo");

            if (evento == null || !evento.Ativo) throw new Exception("Evento inválido ou inativo");
            
            // Validar Assentos
            var assentosSelecionados = evento.Assentos
                .Where(a => assentosIds.Contains(a.Id) && !a.Ocupado)
                .ToList();

            if (assentosSelecionados.Count != assentosIds.Count)
                throw new Exception("Um ou mais assentos selecionados não estão disponíveis.");

            foreach (var assento in assentosSelecionados)
            {
                assento.Ocupado = true;
            }
            evento.LugaresDisponiveis -= assentosSelecionados.Count;

            
            var compra = new Compra
            {
                Cliente = cliente,
                Evento = evento,
                AssentosSelecionados = assentosSelecionados,
                ValorTotal = evento.Valor * assentosSelecionados.Count,
                MetodoPagamento = metodoPagamento,
                DataCompra = DateTime.Now
            };

            _ticketContext.Compras.Add(compra);
            _ticketContext.SaveChangesAsync();
        }

        public void CancelarCompra(int compraId)
        {
            var compra = _ticketContext.Compras
                .Include(c => c.AssentosSelecionados)
                .Include(c => c.Evento)
                .FirstOrDefault(c => c.Id == compraId);

            if (compra == null)
                throw new Exception("Compra não encontrada.");

            // Liberar assentos
            foreach (var assento in compra.AssentosSelecionados)
            {
                assento.Ocupado = false;
            }
            
            if (compra.Evento != null)
            {
                compra.Evento.LugaresDisponiveis += compra.AssentosSelecionados.Count;
            }

                compra.Evento.LugaresDisponiveis += compra.AssentosSelecionados.Count;

            _ticketContext.Compras.Remove(compra);
            _ticketContext.SaveChanges();
        }


        // --- MÉTODOS DE EVENTO ---

        public List<Evento> ListarEventosAtivos()
        {
            return _ticketContext.Eventos
                .Where(e => e.Ativo).ToList();
        }

        public Evento ObterEventoPorId(int id)
        {
            return _ticketContext.Eventos
                .Include(e => e.Assentos)
                .FirstOrDefault(e => e.Id == id);
        }

        public List<Assento> ListarAssentosDoEvento(int eventoId)
        {
            return _ticketContext.Assentos
                .Where(a => a.EventoId == eventoId)
                .ToList();

        }

        // Metodos Evento de ação: POST

        public void CriarEvento(string nome, int quantidadeLugares, decimal valor, string categoria)
        {

            if (string.IsNullOrWhiteSpace(nome))
                throw new Exception("Digite um nome para o evento.");

            if (quantidadeLugares <= 0)
                throw new Exception("Coloque uma quantidade válida de lugares.");

            if (valor < 0)
                throw new Exception("Digite um valor válido.");

            var listaAssentos = new List<Assento>();
            for (int i = 1; i <= quantidadeLugares; i++)
            {
                listaAssentos.Add(new Assento
                {
                    CodigoAssento = $"A{i}",
                    Ocupado = false
                });
            }


                var evento = new Evento
                {
                    Nome = nome,
                    QuantidadeLugares = quantidadeLugares,
                    LugaresDisponiveis = quantidadeLugares, // inicia com todos disponíveis
                    Valor = valor,
                    Categoria = string.IsNullOrEmpty(categoria) ? "Geral" : categoria,
                    Ativo = true,
                    DataCriacao = DateTime.Now,
                    Assentos = listaAssentos,
                };
                
                _ticketContext.Eventos.Add(evento);
                _ticketContext.SaveChanges();
            }
       

        public void DesativarEvento(int id)
        {
            var evento = _ticketContext.Eventos.Find(id);

            if (evento == null) throw new Exception("Evento não encontrado");

            if (!evento.Ativo) throw new Exception("Evento já está desativado");


            evento.Ativo = false;
            _ticketContext.SaveChanges();

        }

        public void SelecionarAssentos(int eventoId, List<string> codigosAssentos)
        {
            var evento = _ticketContext.Eventos
                .Include(e => e.Assentos)
                .FirstOrDefault(e => e.Id == eventoId && e.Ativo);
            
            if (evento == null) throw new Exception("Evento não encontrado ou inativo.");


            // Filtro dos assentos disponíveis
            var assentosDisponiveis = evento.Assentos
            .Where(a => codigosAssentos.Contains(a.CodigoAssento) && !a.Ocupado)
            .ToList();

            if (assentosDisponiveis.Count != codigosAssentos.Count)
           throw new Exception("Um ou mais assentos selecionados não estão disponíveis.");

            if (assentosDisponiveis.Count > evento.LugaresDisponiveis)
         throw new Exception("Número de assentos selecionados excede os lugares disponíveis.");

            // atualiza o status dos assentos para ocupado
            foreach (var assento in assentosDisponiveis)
            {
                assento.Ocupado = true;
            }

            evento.LugaresDisponiveis -= assentosDisponiveis.Count;
          _ticketContext.SaveChanges();
           
        }

        // --- MÉTODOS DE CLIENTE ---

        public List<Cliente> ListarClientesAtivos()
        {
            return _ticketContext.Clientes.Where(c => c.Ativo).ToList();
        }
      
        public void CadastrarCliente(string nome, string email, int idade)
        {
            if (string.IsNullOrEmpty(nome)) throw new ArgumentException("Digite um nome válido");

            if (idade < 18) throw new ArgumentException("É necessário ser maior de 18 anos");

            if (string.IsNullOrEmpty(email)) /*|| !email.Contains("@"))*/ throw new ArgumentException("Digite um E-mail válido");

            var clientes = new Cliente
            {
                Nome = nome,
                Email = email,
                Idade = idade,
                Ativo = true,
                DataCadastro = DateTime.Now,
            };
            
            _ticketContext.Clientes.Add(clientes);
            _ticketContext.SaveChanges();
        }

        public string DesativarCliente(int id)
        {
            var cliente = _ticketContext.Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null) throw new Exception("Cliente não encontrado");
            if (!cliente.Ativo) throw new Exception("O cliente não está ativo");

            cliente.Ativo = false;

            _ticketContext.SaveChanges();
            return $"O cliente: {cliente.Nome} foi desativado com sucesso";
        }

    }
}
