using Microsoft.EntityFrameworkCore;
using GestionBudget_V2.Models;

namespace GestionBudget_V2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Administrateur> Administrateurs { get; set; }
        public DbSet<Comptable> Comptables { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Depense> Depenses { get; set; }
        public DbSet<Rapport> Rapports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Administrateur>()
                .HasIndex(a => a.Email)
                .IsUnique();
                
            modelBuilder.Entity<Comptable>()
                .HasIndex(c => c.Email)
                .IsUnique();
        }
    }
}