using DocumentFormat.OpenXml.Office2010.Excel;
using InvoiceManagementSystem.Data;
using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mono.TextTemplating;
using System.Linq;
using System.Linq.Expressions;

namespace InvoiceManagementSystem.Repositories.Implementations
{
    public class ProductRepository(ApplicationDbContext _context) : IProductRepository
    {
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products.ToListAsync();

        }
        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }
        public async Task<Product> CreateProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            return product;

        }
        public Product UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            return product;
        }
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is not null)
            {
                _context.Products.Remove(product);
                return true;
            }
            return false;
        }

        public async Task<List<SelectListItem>> GetProductSelectListItemsAsync()
        {
            var products = await _context.Products.Select(p => new SelectListItem
            {
                Value = p.ProductId.ToString(),
                Text = p.ProductName
            }).ToListAsync();
            return products;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Products.CountAsync();
        }
    }
}
