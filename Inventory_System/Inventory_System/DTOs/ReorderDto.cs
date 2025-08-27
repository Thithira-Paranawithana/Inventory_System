using Inventory_System.Entities;

namespace Inventory_System.DTOs
{
    public class ReorderDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int RecommendedQuantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    }
}
