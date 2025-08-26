using Inventory_System.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_System.DTOs
{
    public class SalesTransactionDto
    {
        [Required(ErrorMessage = "Store Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Store Id should be a positive integer")]
        public int StoreId { get; set; }

        [Required(ErrorMessage = "Product Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Product Id should be a positive integer")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity should be a positive integer")]
        public int Quantity { get; set; }

        [Required]
        public DateTime SaleDate { get; set; } 
    }
}
