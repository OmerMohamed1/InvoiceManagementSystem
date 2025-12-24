namespace InvoiceManagementSystem.ViewMoldel
{
    public class ReportViewMoldel
    {
        public int InvoiceId { get; set; }
        public string ProductName { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public decimal TotalAmount { get; set; }
    }
}
