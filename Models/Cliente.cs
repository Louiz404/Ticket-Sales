using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TicketSales.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }
        public int Idade { get; set; }

        [Required] 
        [EmailAddress(ErrorMessage = "O E-mail inválido.")]
        public string Email { get; set; }
        
        public bool Ativo { get; set; } = true;
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }

    public class GerenciadorClientes
    {
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
        private int proximoId = 1;

        private void ValidarCliente(string nome, string email, int idade)
        {
             if (string.IsNullOrEmpty(nome))

                throw new ArgumentException("Digite um nome válido");

            if (idade < 18)

                throw new ArgumentException("É necessário ser maior de 18 anos");

            if (string.IsNullOrEmpty(email)) /*|| !email.Contains("@"))*/

                throw new ArgumentException("Digite um E-mail válido");

        }
        public void CadastrarCliente(string nome, string email, int idade)
        {
            ValidarCliente(nome, email, idade);

            var clientes = new Cliente
            {
                Id = proximoId++,
                Nome = nome,
                Email = email,
                Idade = idade,
                Ativo = true,
                DataCadastro = DateTime.Now,
            };
            Clientes.Add(clientes);
        }

        public string DesativarCliente(int id)
        {
            var cliente = Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null) return "Cliente não encontrado";
            if (!cliente.Ativo) return "O cliente não está ativo";

            cliente.Ativo = false;
            return $"O cliente: {cliente.Nome} foi desativado com sucesso";
        }
        
        public List<Cliente> ListarClientesAtivos()
        {
            return Clientes.Where(c => c.Ativo).ToList();
        }
        
        public string AtualizarDados(int id, string nome, int idade, string email)
        {
            var cliente = Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
           return "Cliente não encontrado";

            ValidarCliente(nome, email, idade);
            
            cliente.Nome = nome;
            cliente.Idade = idade;
            cliente.Email = email;

            return $"Os dados do {cliente.Nome} foram alterados com sucesso!";


        }

    }

}
