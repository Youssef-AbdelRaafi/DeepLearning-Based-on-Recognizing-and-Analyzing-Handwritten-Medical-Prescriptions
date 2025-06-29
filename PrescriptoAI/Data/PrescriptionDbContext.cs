using PrescriptoAI.Models;
using Microsoft.EntityFrameworkCore;

namespace PrescriptoAI.Data
{
    public class PrescriptionDbContext : DbContext
    {
        public PrescriptionDbContext(DbContextOptions<PrescriptionDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pharmacist>()
                .HasMany(p => p.Prescriptions)
                .WithOne(p => p.Pharmacist)
                .HasForeignKey(p => p.PharmacistId);
        }
    }
}