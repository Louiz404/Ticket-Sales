using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TicketSales.Models
{
    public enum TiposDePagamento
    {
        Pix = 1,
        
        [Display (Name = "Cartão de Crédito")]
        CartaoCredito = 2,
        
        [Display(Name = "Cartão de Débito")]
        CartaoDebito = 3,
        
        Boleto = 4,
       
    }
    public class Compra
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Código chave")]
        public int Id { get; set; }
        
        public DateTime DataCompra { get; set; } = DateTime.Now;

        [Display(Name = "Metodo de Pagamento")]
        [Required(ErrorMessage = "Por favor, Selecione uma opção {0}.")]
        public TiposDePagamento MetodoPagamento { get; set; }

        [Display(Name = "Valor Total")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ValorTotal { get; set; }

        // Relacionamentos
        public Cliente Cliente { get; set; }
        public int ClienteId { get; set; }
        
        public int EventoId { get; set; }
        public Evento Evento { get; set; }
     
        public List<Assento> AssentosSelecionados { get; set; } = new List<Assento>();
    }

    public class GerenciadorCompra
    {
        public List<Compra> Compras { get; set; } = new List<Compra>();
        private int proximoId = 1;

        public Compra RegistrarCompra(Cliente cliente, Evento evento, List<Assento> assentos, TiposDePagamento MetodoPagamento)
        {
            if (!cliente.Ativo)
                throw new Exception("O cliente precisa estar ativo para comprar");

            if (!evento.Ativo)
                throw new Exception("O evento está inativo");
                
            var assentosSelecionados = evento.Assentos
                .Where(a => assentos.Contains(a) && !a.Ocupado)
                .ToList();


            if (evento.LugaresDisponiveis < assentos.Count)
                throw new Exception("Um ou mais assentos já estão ocupados ou inexistem.");


            foreach (var assento in assentosSelecionados)
            {
                assento.Ocupado = true;
            }
            evento.LugaresDisponiveis -= assentosSelecionados.Count;

           
            var compra = new Compra
            {
                Id = proximoId++,
                Cliente = cliente,
                Evento = evento,
                AssentosSelecionados = assentos,
                ValorTotal = evento.Valor * assentos.Count,
                DataCompra = DateTime.Now,
                MetodoPagamento = MetodoPagamento
            };
            Compras.Add(compra);

            Console.WriteLine($"Compra realizada com sucesso! {cliente.Nome}. {assentosSelecionados.Count} Assento(s) reservados(s) no evento: {evento.Nome}. Valor total R$ {compra.ValorTotal}");
            return compra;
        }

        public List<Compra> ListarComprasCliente(int clienteId)
        {
            return Compras.Where(c => c.Cliente.Id == clienteId).ToList();

        }

        public void CancelarCompra(int compraId)
        {
            var compra = Compras.FirstOrDefault(c => c.Id == compraId);
            if (compra == null)
                throw new Exception("Compra não encontrada.");

            foreach (var assento in compra.AssentosSelecionados)
            {
                assento.Ocupado = false;
            }
            compra.Evento.LugaresDisponiveis += compra.AssentosSelecionados.Count;

            Compras.Remove(compra);
            Console.WriteLine($"Compra {compraId} cancelada com sucesso, e assentos liberados.");
        }

        public void ListarTodasCompras()
        {
            foreach (var compra in Compras)
            {
                Console.WriteLine($"Compra ID: {compra.Id}, Cliente: {compra.Cliente.Nome}, Evento: {compra.Evento.Nome}, Assentos: {compra.AssentosSelecionados.Count}, Valor Total: R$ {compra.ValorTotal}, Data: {compra.DataCompra}");
            }
        }
        
        public List<Compra> ListarComprasPorEvento(int eventoId)
        {
            return Compras.Where(c => c.Evento.Id == eventoId).ToList();
        }

    }
}