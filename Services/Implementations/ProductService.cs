using DocumentFormat.OpenXml.Office2010.Excel;
using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.Repositories.Interfaces;
using InvoiceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace InvoiceManagementSystem.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public ProductService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            string cacheKey = "productList";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Product> products))
            {
                products = await _unitOfWork.Products.GetProductsAsync();

                if (products is null)
                    return default;

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, products, cacheOptions);
            }

            return products;

        }
        public async Task<Product> GetProductByIdAsync(int id)
        {
            if (id <= 0)
                return default;

            var product = await _unitOfWork.Products.GetProductByIdAsync(id);

            if (product is null)
                return default;

            return product;
        }
        public async Task<Product> CreateProductAsync(Product product)
        {
            if (product is null)
                return default;

            await _unitOfWork.Products.CreateProductAsync(product);
            await _unitOfWork.CompleteAsync();
            _cache.Remove("productList");
            return product;
        }
        public async Task<Product> UpdateProductAsync(Product product)
        {
            if (product is null)
                return default;

            _unitOfWork.Products.UpdateProductAsync(product);
            await _unitOfWork.CompleteAsync();
            _cache.Remove("productList");
            return product;
        }
        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id <= 0)
                return default;

            await _unitOfWork.Products.DeleteProductAsync(id);
            await _unitOfWork.CompleteAsync();
            _cache.Remove("productList");

            return true;
        }

        public async Task<List<SelectListItem>> GetProductSelectListItemsAsync()
        {
            return await _unitOfWork.Products.GetProductSelectListItemsAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _unitOfWork.Products.CountAsync();
        }
    }
}
