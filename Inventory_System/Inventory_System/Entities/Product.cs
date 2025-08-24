using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_System.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SKU { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Column(TypeName ="decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int MinStockLevel { get; set; }

        [Required]
        public int LeadTimeDays { get; set; }

        [Required]
        public int MaxStorageQty { get; set; }

        public List<Inventory> Inventories { get; set; } = new();
        public List<SalesTransaction> SalesTransactions { get; set; } = new();
        public List<ReorderRecommendation> ReorderRecommendations { get; set; } = new();
    }
}
