using DPowerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPowerAPI.Data
{
    public class M5LDbContext : DbContext
    {
        public M5LDbContext(DbContextOptions<M5LDbContext> options)
            : base(options)
        {
        }
       
        public DbSet<STK> STK { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<STK>()
                .HasKey(s => s.STKautoNo);
        }

    }
}
