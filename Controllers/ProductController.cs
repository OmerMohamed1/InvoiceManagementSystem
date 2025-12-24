using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManagementSystem.Controllers
{
    [Authorize]
    public class ProductController(IProductService _productService) : Controller
    {

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetProductsAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> LoadProductsPartial()
        {
            var products = await _productService.GetProductsAsync();
            return PartialView("_ProductTablePartial", products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateProductPartial");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (product is null)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _productService.CreateProductAsync(product);
                TempData["Create"] = "Item has been created successfully";
                return Json(new { success = true, message = TempData["Create"] });
            }

            return Json(new { success = false, errorMessage = "Invalid data" });
        }


        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            return PartialView("_EditProductPartial", product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (product is null)
                return BadRequest();

            //var existingProduct = await _productService.GetProductByIdAsync(product.ProductId);
            //if (existingProduct is null)
            //{
            //    return NotFound();
            //}

            if (ModelState.IsValid)
            {
                await _productService.UpdateProductAsync(product);
                TempData["Update"] = "Item has Update Succesfully";
                return Json(new { success = true, message = TempData["Update"] });
            }

            return Json(new { success = false, errorMessage = "Invalid data" });
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            return PartialView("_DetailsProductPartial", product);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var product = await _productService.GetProductByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
                return NotFound();

            await _productService.DeleteProductAsync(id);
            return RedirectToAction("Index");
        }

        //private List<Product> GetCachedProducts()
        //{
        //    string cacheKey = "productList";
        //    if (!_cache.TryGetValue(cacheKey, out List<Product> products))
        //    {
        //        products = _context.Products.ToList();

        //        if (products is null)
        //            return default;

        //        var cacheOptions = new MemoryCacheEntryOptions()
        //            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
        //            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        //        _cache.Set(cacheKey, products, cacheOptions);
        //    }

        //    return products;
        //}

    }
}
