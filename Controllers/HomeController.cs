using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InvoiceManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly IInvoiceService _invoiceService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, IInvoiceService invoiceService)
        {
            _logger = logger;
            _productService = productService;
            _invoiceService = invoiceService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.InvoiceCounts = await _invoiceService.CountAsync();
            ViewBag.TodayInvoiceCounts = await _invoiceService.CountAsync(i => i.InvoiceDate.Date == DateTime.Today);
            ViewBag.ProductCounts = await _productService.CountAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
