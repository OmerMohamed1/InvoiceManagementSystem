using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementSystem.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }
    }
}
