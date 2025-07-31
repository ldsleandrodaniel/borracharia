using borracharia.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace borracharia.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<ServicoEfetuado> ServicosEfetuados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração do relacionamento (apenas de ServicoEfetuado para Servico)
            modelBuilder.Entity<ServicoEfetuado>()
                .HasOne(se => se.Servico)           // ServicoEfetuado tem um Servico
                .WithMany()                         // Servico NÃO tem coleção de ServicosEfetuados
                .HasForeignKey(se => se.ServicoId)  // Chave estrangeira
                .OnDelete(DeleteBehavior.Restrict);  // Comportamento de delete

            // Configurações de precisão decimal
            modelBuilder.Entity<Servico>()
                .Property(s => s.Valor)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<ServicoEfetuado>()
                .Property(se => se.Valor)
                .HasColumnType("decimal(10,2)");
        }
       
    }
}
