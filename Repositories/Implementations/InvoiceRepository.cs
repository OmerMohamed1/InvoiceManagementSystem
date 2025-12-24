using InvoiceManagementSystem.Data;
using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace InvoiceManagementSystem.Repositories.Implementations
{
    public class InvoiceRepository(ApplicationDbContext _context) : IInvoiceRepository
    {
        public async Task<IEnumerable<Invoice>> GetInvoicesAsync(int pageIndex, int pageSize)
        {
            var invoices = await _context.Invoices
                .Include(i => i.InvoiceItems)
               .Skip((pageIndex - 1) * pageSize) // تخطي الصفوف السابقة
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
            return invoices;
        }
        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(s => s.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }
        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            var addedInvoice = await _context.Invoices.AddAsync(invoice);
            return addedInvoice.Entity;
        }

        public Invoice UpdateInvoiceAsync(Invoice invoice)
        {
            var updateIvoice = _context.Invoices.Update(invoice);
            return updateIvoice.Entity;
        }
        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice is not null)
            {
                _context.Invoices.Remove(invoice);
                return true;
            }
            return false;
        }

        public async Task<int> CountAsync(Expression<Func<Invoice, bool>> match)
        {
            return await _context.Invoices.CountAsync(match);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Invoices.CountAsync();
        }
    }
}
