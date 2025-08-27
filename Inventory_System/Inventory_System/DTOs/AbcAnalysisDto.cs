namespace Inventory_System.DTOs
{
    public class AbcAnalysisDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; } 
        public double RevenuePercentage { get; set; }
        public double CumulativePercentage { get; set; } 
        public char AbcCategory { get; set; } 


    }
}
