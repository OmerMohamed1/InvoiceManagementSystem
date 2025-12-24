using InvoiceManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq.Expressions;

namespace InvoiceManagementSystem.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Product UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<int> CountAsync();
        Task<List<SelectListItem>> GetProductSelectListItemsAsync();

    }
}
