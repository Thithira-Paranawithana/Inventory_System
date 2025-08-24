using Inventory_System.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory_System.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<SalesTransaction> SalesTransactions { get; set; }
        public DbSet<ReorderRecommendation> ReorderRecommendations { get; set; }

    }
}
