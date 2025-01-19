using GeoPagos.Authorization.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GeoPagos.Authorization.Infraestructure
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public virtual DbSet<AuthorizationRequest> AuthorizationRequest { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración específica para AuthorizationRequest
            modelBuilder.Entity<AuthorizationRequest>(entity =>
            {
                entity.ToTable("AuthorizationRequest");
                entity.HasKey(e => e.Id); // Clave primaria

                entity.Property(e => e.TransactionId);
                entity.Property(e => e.CustomerName);
                entity.Property(e => e.CustomerType);
                entity.Property(e => e.TransactionType);

                entity.Property(e => e.Amount)
                      .HasPrecision(18, 2); // Precisión para decimales

                entity.Property(e => e.CreatedAt);

                entity.Property(e => e.Status);
            });

            modelBuilder.Entity<AuthorizationRequestApproved>(entity =>
            {
                entity.ToTable("AuthorizationRequestApproved");
                entity.HasKey(e => e.Id); // Clave primaria

                entity.Property(e => e.TransactionId);
                entity.Property(e => e.CustomerName);
                entity.Property(e => e.Amount)
                      .HasPrecision(18, 2); // Precisión para decimales
            });

            // Configuraciones adicionales (si aplica)
        }    
    }
}
