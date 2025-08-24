using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_System.Entities
{
    public class SalesTransaction
    {
        [Key]
        public int Id { get; set; }

        public int StoreId { get; set; }
        public Store Store { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName ="decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;


    }
}
