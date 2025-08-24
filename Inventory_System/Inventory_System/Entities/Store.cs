using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Inventory_System.Entities
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Address { get; set; }

        public List<User> Users { get; set; } = new();
        public List<Inventory> Inventories { get; set; } = new();
        public List<SalesTransaction> SalesTransactions { get; set; } = new();
        public List<ReorderRecommendation> ReorderRecommendations { get; set; } = new();

    }
}
