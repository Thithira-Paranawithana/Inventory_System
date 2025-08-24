using System.ComponentModel.DataAnnotations;

namespace Inventory_System.Entities
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int StoreId { get; set; }
        public Store Store { get; set; }

        public int CurrentStock { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
