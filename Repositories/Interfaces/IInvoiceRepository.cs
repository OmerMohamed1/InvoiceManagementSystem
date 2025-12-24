using InvoiceManagementSystem.Models;
using System.Linq.Expressions;

namespace InvoiceManagementSystem.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetInvoicesAsync(int pageIndex, int pageSize);
        Task<Invoice> GetInvoiceByIdAsync(int id);
        Task<Invoice> CreateInvoiceAsync(Invoice invoice);
        Invoice UpdateInvoiceAsync(Invoice invoice);
        Task<int> CountAsync(Expression<Func<Invoice, bool>> match);
        Task<int> CountAsync();
        Task<bool> DeleteInvoiceAsync(int id);
    }
}
