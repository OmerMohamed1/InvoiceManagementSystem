using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementSystem.Models
{
    public class InvoiceItem
    {
        public int InvoiceItemId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public virtual Invoice Invoice { get; set; }
        public virtual Product Product { get; set; }

    }
}
