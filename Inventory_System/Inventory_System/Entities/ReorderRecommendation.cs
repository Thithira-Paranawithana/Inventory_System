using System.ComponentModel.DataAnnotations;

namespace Inventory_System.Entities
{
    public class ReorderRecommendation
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int StoreId { get; set; }
        public Store Store { get; set; }

        public int RecommendedQuantity { get; set; }

        public string Reason { get; set; }

        public DateTime GeneratedDate { get; set; }

        public string Status { get; set; }


    }
}
