using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using TicketSales.Models;

namespace TicketSales.Models
{

    public class Evento
    {
        [Key]
        public int Id { get; set; }
        
        [Display(Name = "Nome do Evento: ")]
        [Required(ErrorMessage = "Por favor, preencha o campo {0}.")]
        public string Nome { get; set; }
        
        [Display(Name = "Quantidade de Lugares")]
        [Required(ErrorMessage = "Por favor, preencha o campo {0}.")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero.")]
        public int QuantidadeLugares { get; set; }
        
        [Display(Name = "Quantidade de Lugares")]
        [Required(ErrorMessage = "Por favor, preencha o campo {0}.")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero.")]
        public int LugaresDisponiveis { get; set; }

        [Display(Name = "Valor do evento: ")]
        [Required(ErrorMessage = "Por favor, preencha o campo {0}.")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero.")]
        public decimal Valor { get; set; }
        
        public string Categoria { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public List<Assento> Assentos { get; set; } = new List<Assento>();
        public string? Imagem { get; set; }
    }
    public class Assento
    {
        [Key]
        public string CodigoAssento { get; set; }
        public bool Ocupado { get; set; } = false;
    }

    public class GerenciadorEventos
    {
        public List<Evento> Eventos { get; set; } = new List<Evento>();
        private List<Assento> GerarAssentos(int quantidade)
        {
            var assentos = new List<Assento>();
            for (int i = 1; i <= quantidade; i++)
            {
                assentos.Add(new Assento { CodigoAssento = $"A{i}" });
            }
            return assentos;
        }
        private int proximoId = 1;

        private string ValidarEvento(string nome, int quantidadeLugares, decimal valor, string categoria)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Digite um nome para o evento.");

            if (quantidadeLugares <= 0)
                throw new ArgumentException("Coloque uma quantidade válida de lugares.");

            if (valor <= 0)
                throw new ArgumentException("Digite um valor R$ válido para o evento.");

            return string.IsNullOrWhiteSpace(categoria) ? "Geral" : categoria;

        }

        public void CriarEvento(string nome, int quantidadeLugares, decimal valor, string categoria)
        {

            ValidarEvento(nome, quantidadeLugares, valor, categoria);


            var evento = new Evento
            {
                Id = proximoId++,
                Nome = nome,
                QuantidadeLugares = quantidadeLugares,
                LugaresDisponiveis = quantidadeLugares, // inicia com todos disponíveis
                Valor = valor,
                Categoria = categoria,
                Ativo = true,
                DataCriacao = DateTime.Now,
                Assentos = GerarAssentos(quantidadeLugares)
            };
            Eventos.Add(evento);

        }

        public bool DesativarEvento(int id)
        {
            var evento = Eventos.FirstOrDefault(e => e.Id == id);

            if (evento == null)
            {
                Console.WriteLine($"Evento não encontrado");

                return false;
            }

            if (!evento.Ativo)
            {
                Console.WriteLine($"O evento não está ativo");
                return false;
            }

            evento.Ativo = false;
            Console.WriteLine($"O Evento: {evento.Nome} foi desativado com sucesso.");
            return true;

        }

        public List<Evento> ListarEventosAtivos()
        {
            return Eventos.Where(e => e.Ativo).ToList();
        }
        public bool ReservarLugares(int eventoId, int quantidade)
        {
            var evento = Eventos.FirstOrDefault(e => e.Id == eventoId && e.Ativo);
            if (evento == null)
            {
                Console.WriteLine("O evento não está disponivel e/ou não foi encontrado");
                return false;
            }

            if (evento.LugaresDisponiveis < quantidade)
            {
                Console.WriteLine("Não há lugares suficientes disponiveis");
                return false;
            }

            evento.LugaresDisponiveis -= quantidade;
            Console.WriteLine($"{quantidade} lugares reservados com sucesso");
            return true;
        }

        public bool SelecionarAssentos(int eventoId, List<string> codigosAssentos)
        {
            var evento = Eventos.FirstOrDefault(e => e.Id == eventoId && e.Ativo);
            if (evento == null)
            {
                Console.WriteLine("Evento não encontrado ou inativo");
                return false;
            }

            var assentosDisponiveis = evento.Assentos
            .Where(a => codigosAssentos.Contains(a.CodigoAssento) && !a.Ocupado)
            .ToList();

            if (assentosDisponiveis.Count != codigosAssentos.Count)
            {
                Console.WriteLine("Um ou mais assentos já estão ocupados ou inexistem.");
                return false;
            }

            if (assentosDisponiveis.Count > evento.LugaresDisponiveis)
            {
                Console.WriteLine("Não há lugares suficientes disponíveis para reservar os assentos selecionados.");
                return false;
            }

            foreach (var assento in assentosDisponiveis)
            {
                assento.Ocupado = true;
            }

            evento.LugaresDisponiveis -= assentosDisponiveis.Count;
            Console.WriteLine($"Assentos {string.Join(", ", codigosAssentos)} reservados com sucesso !");
            return true;
        }

        public void MostrarAssentos(int eventoId)
        {
            var evento = Eventos.FirstOrDefault(e => e.Id == eventoId);
            if (evento == null)
            {
                Console.WriteLine("Evento não encontrado.");
                return;
            }

            Console.WriteLine($"Assentos para o evento: {evento.Nome}");
            foreach (var assento in evento.Assentos)
            {
                Console.ForegroundColor = assento.Ocupado ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine($"Assento: {assento.CodigoAssento} - {(assento.Ocupado ? "Ocupado" : "Disponível")}");
                Console.ResetColor();


            }
            Console.WriteLine();
        }
    }
}