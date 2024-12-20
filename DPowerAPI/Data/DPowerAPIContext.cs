
using DPowerAPI.models;
using Microsoft.EntityFrameworkCore;

namespace DPowerAPI.Data
{
    public class DPowerAPIContext : DbContext
    {
        public DPowerAPIContext (DbContextOptions<DPowerAPIContext> options)
            : base(options)
        {
        }

        public DbSet<DPowerAPI.models.Menu> Menu { get; set; } = default!;
        public DbSet<DPowerAPI.models.UserMenu> UserMenu { get; set; } = default!;
        public DbSet<DPowerAPI.models.RolePermissions> RolePermissions { get; set; } = default!;
        public DbSet<DPowerAPI.models.UserRoles> UserRoles { get; set; } = default!;
        public DbSet<DPowerAPI.models.Permissions> Permissions { get; set; } = default!;
        public DbSet<DPowerAPI.models.Roles> Roles { get; set; } = default!;
        public DbSet<DPowerAPI.models.User> User{ get; set; } = default!;
        public DbSet<DPowerAPI.models.BalanceCustomer> BalanceCustomer { get; set; } = default!;
        public DbSet<DPowerAPI.models.BalanceInventory> BalanceInventory { get; set; } = default!;
        public async Task CallspGetBalanceCustomer()
        {
            // เรียกใช้ Stored Procedure โดยไม่ต้องการข้อมูลผลลัพธ์
            await Database.ExecuteSqlRawAsync("EXEC GetBalanceCustomers");
        }
        public async Task CallspGetBalanceInventory()
        {
            await Database.ExecuteSqlRawAsync("EXEC GetBalanceInventory");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set name tatble in models
            modelBuilder.Entity<DPowerAPI.models.BalanceCustomer>().ToTable("TEM_BALANCE_CUSTOMER");
            modelBuilder.Entity<DPowerAPI.models.BalanceInventory>().ToTable("TEM_INVENTORY");

            modelBuilder.Entity<RolePermissions>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermissions>()
                .HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermissions>()
                .HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRoles>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            
            modelBuilder.Entity<UserRoles>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId);

           
            modelBuilder.Entity<UserRoles>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<UserMenu>()
                .HasKey(um => new { um.UserId, um.MenuId });


            modelBuilder.Entity<UserMenu>()
                .HasOne(um => um.User)
                .WithMany()
                .HasForeignKey(um => um.UserId);


            modelBuilder.Entity<UserMenu>()
                .HasOne(um => um.Menu)
                .WithMany()
                .HasForeignKey(um => um.MenuId);

        }
        
    }
}
