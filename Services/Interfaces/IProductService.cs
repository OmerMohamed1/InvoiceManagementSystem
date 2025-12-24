using InvoiceManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InvoiceManagementSystem.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<int> CountAsync();
        Task<List<SelectListItem>> GetProductSelectListItemsAsync();
    }
}
