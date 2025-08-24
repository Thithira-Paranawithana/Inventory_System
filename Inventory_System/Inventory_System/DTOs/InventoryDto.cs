using Inventory_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_System.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public decimal Price { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsLowStock => CurrentStock <= MinStockLevel;
    }
}
