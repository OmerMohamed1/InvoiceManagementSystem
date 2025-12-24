using Microsoft.AspNetCore.Mvc.Rendering;

namespace InvoiceManagementSystem.ViewMoldel
{
    public class InvoiceViewModel
    {
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public decimal TotalAmount => InvoiceItems.Sum(item => item.TotalPrice);
        public string QRCodeImage { get; set; } = "Print Invoice With QR Code";
        public string ProductName { get; set; }
        public List<InvoiceItemViewModel> InvoiceItems { get; set; } = new List<InvoiceItemViewModel>();
        public IEnumerable<SelectListItem> Products { get; set; }

    }
}
