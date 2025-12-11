using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }
        
        [Display(Name = "Quantidade de Lugares")]
        [Required]
        public int QuantidadeLugares { get; set; }
        
        [Display(Name = "Quantidade de Lugares")]
        public int LugaresDisponiveis { get; set; }

        [Display(Name = "Valor")]
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Valor { get; set; }

        public string? OrganizadorId { get; set; }
        
        public string Categoria { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public string? Imagem { get; set; }

        public List<Assento> Assentos { get; set; } = new List<Assento>();

    }
    public class Assento
    {
        [Key]
        public int Id { get; set; }
        
        [Display(Name = "Código do Assento")]
        [Required(ErrorMessage = "Por favor, preencha o campo {0}.")]
        public string CodigoAssento { get; set; }
        public bool Ocupado { get; set; } = false;

        // Relacionamentos
        public Evento Evento { get; set; }
        public int EventoId { get; set; }
        
        public int? CompraId { get; set; }
        public Compra? Compra { get; set; }
    }   
}