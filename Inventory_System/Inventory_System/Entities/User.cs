using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Inventory_System.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [PasswordPropertyText]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

        public int StoreId { get; set; }
        public Store Store { get; set; }
    }
}
