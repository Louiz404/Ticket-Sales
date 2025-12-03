using Microsoft.EntityFrameworkCore;
using TicketSales.Models;

namespace TicketSales.Data
{
    public class TicketContext : DbContext
    {

        public TicketContext(DbContextOptions<TicketContext> options) : base(options)
        { 
        }

        public DbSet<Compra> Compras { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Assento> Assentos { get; set; }

    }
}
