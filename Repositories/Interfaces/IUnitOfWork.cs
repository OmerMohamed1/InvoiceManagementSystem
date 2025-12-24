namespace InvoiceManagementSystem.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        public IInvoiceRepository Invoices { get; }
        public IProductRepository Products { get; }
        Task<int> CompleteAsync();
    }
}
