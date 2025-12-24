using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.ViewMoldel;
using System.Linq.Expressions;

namespace InvoiceManagementSystem.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<(IEnumerable<Invoice> Invoices, bool HasMore)> GetInvoicesAsync(int pageIndex, int pageSize);
        Task<Invoice> GetInvoiceByIdAsync(int id);
        Task<Invoice> CreateInvoiceAsync(InvoiceViewModel model);
        Task<Invoice> UpdateInvoiceAsync(InvoiceViewModel model);
        Task<bool> DeleteInvoiceAsync(int id);
        Task<int> CountAsync(Expression<Func<Invoice, bool>> match);
        Task<int> CountAsync();
        Task<InvoiceViewModel> PrintInvoiceAsync(int invoiceId);
    }
}
