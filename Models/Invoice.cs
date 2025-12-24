using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementSystem.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    }
}
