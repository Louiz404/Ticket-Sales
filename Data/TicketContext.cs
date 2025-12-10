using Microsoft.EntityFrameworkCore;
using TicketSales.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace TicketSales.Data
{
    public class TicketContext : IdentityDbContext<IdentityUser>
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
