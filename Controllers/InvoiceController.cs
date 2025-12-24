using DocumentFormat.OpenXml.Office2010.Excel;
using InvoiceManagementSystem.Data;
using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.Services.Interfaces;
using InvoiceManagementSystem.ViewMoldel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using QRCoder;

namespace InvoiceManagementSystem.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IProductService _productService;

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public InvoiceController(ApplicationDbContext context, IInvoiceService invoiceService, IProductService productService)
        {
            _context = context;

            _invoiceService = invoiceService;
            _productService = productService;
        }



        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5)
        {
            var (invoices, hasMore) = await _invoiceService.GetInvoicesAsync(pageIndex, pageSize);
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageSize = pageSize;
            ViewBag.HasMore = hasMore;

            var products = await _productService.GetProductSelectListItemsAsync();

            ViewBag.ProductsJson = JsonConvert.SerializeObject(products);

            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> LoadInvoicesPartial(int pageIndex = 1, int pageSize = 5)
        {
            var (invoices, hasMore) = await _invoiceService.GetInvoicesAsync(pageIndex, pageSize);
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageSize = pageSize;
            ViewBag.HasMore = hasMore;

            return PartialView("_InvoicesTablePartial", invoices);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var invoiceViewModel = new InvoiceViewModel
            {
                Products = await _productService.GetProductSelectListItemsAsync()
            };

            return PartialView("_CreateInvoicePartial", invoiceViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceViewModel model)
        {
            if (model is null)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var invoice = await _invoiceService.CreateInvoiceAsync(model);

                TempData["Create"] = "Invoice has Create Succesfully";

                return Json(new { success = true, message = TempData["Create"], invoiceId = invoice.InvoiceId });
            }

            return Json(new { success = false, errorMessage = "Invalid data" });
            //    return RedirectToAction("PrintInvoiceSmall", "Invoice", new { id = invoice.InvoiceId });

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);

            if (invoice is null)
                return NotFound();

            var invoiceViewModel = ConvertInvoiceToInvoiceViewModel(invoice);

            return PartialView("_EditInvoicePartial", invoiceViewModel);
            // return View(invoiceViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InvoiceViewModel model)
        {
            if (model is null)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _invoiceService.UpdateInvoiceAsync(model);

                TempData["Update"] = "Invoice has Update Succesfully";
                return Json(new { success = true, message = TempData["Update"] });
            }

            return Json(new { success = false, errorMessage = "Invalid data" });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice is null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
                return BadRequest();

            await _invoiceService.DeleteInvoiceAsync(id);

            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Details(int invoiceId)
        {

            if (invoiceId <= 0)
                return BadRequest();

            var invoiceViewModel = await _invoiceService.PrintInvoiceAsync(invoiceId);

            if (invoiceViewModel is null)
                return NotFound();

            return PartialView("_DetailsInvoicePartial", invoiceViewModel);
        }

        [Route("Invoice/PrintInvoiceSmall/{invoiceId}")]
        public async Task<ActionResult> PrintInvoiceSmall(int invoiceId)
        {
            if (invoiceId <= 0)
                return BadRequest();

            var invoiceViewModel = await _invoiceService.PrintInvoiceAsync(invoiceId);

            if (invoiceViewModel is null)
                return NotFound();

            return PartialView("_PrintInvoiceSmallPartial", invoiceViewModel);
        }

        private InvoiceViewModel ConvertInvoiceToInvoiceViewModel(Invoice invoice)
        {
            return new InvoiceViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceDate = invoice.InvoiceDate,
                InvoiceItems = invoice.InvoiceItems.Select(i => new InvoiceItemViewModel
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,

                }).ToList(),

                Products = _context.Products.Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.ProductName
                }).ToList()
            };
        }


    }
}








































































































































//public IActionResult GeneratePdfA4(int invoiceId)
//{
//    var invoice = GetInvoiceById(invoiceId);

//    using (var stream = new MemoryStream())
//    {
//        var document = new PdfDocument();
//        var page = document.AddPage();
//        page.Size = PdfSharpCore.PageSize.A4;
//        var graphics = XGraphics.FromPdfPage(page);

//        DrawInvoice(graphics, invoice);

//        document.Save(stream, false);
//        return File(stream.ToArray(), "application/pdf", "InvoiceA4.pdf");
//    }
//}
//public IActionResult GeneratePdfSmall(int invoiceId)
//{
//    var invoice = GetInvoiceById(invoiceId);

//    using (var stream = new MemoryStream())
//    {
//        var document = new PdfDocument();
//        var page = document.AddPage();
//        page.Width = XUnit.FromMillimeter(80);  // عرض الصفحة
//        page.Height = XUnit.FromMillimeter(150); // ارتفاع الصفحة
//        var graphics = XGraphics.FromPdfPage(page);

//        DrawInvoiceSmall(graphics, invoice);

//        document.Save(stream, false);
//        return File(stream.ToArray(), "application/pdf", "InvoiceSmall.pdf");
//    }
//}
//private void DrawInvoiceSmall(XGraphics graphics, Invoice invoice)
//{
//    var titleFont = new XFont("Verdana", 10, XFontStyle.Bold);
//    var headerFont = new XFont("Verdana", 8, XFontStyle.Bold);
//    var smallFont = new XFont("Verdana", 6, XFontStyle.Regular);

//    int margin = 10;
//    int rowHeight = 15;
//    int tableTop = 20;
//    int tableWidth = (int)graphics.PageSize.Width - 2 * margin;

//    // Header
//    graphics.DrawString("Invoice Details", titleFont, XBrushes.Black, new XRect(0, 0, graphics.PageSize.Width, 20), XStringFormats.TopCenter);
//    graphics.DrawLine(XPens.Black, 0, 20, graphics.PageSize.Width, 20);

//    // Invoice Details
//    graphics.DrawString($"Invoice Number: {invoice.InvoiceId}", headerFont, XBrushes.Black, new XRect(margin, 25, graphics.PageSize.Width - 2 * margin, 15), XStringFormats.TopLeft);
//    graphics.DrawString($"Invoice Date: {invoice.InvoiceDate:dd-MM-yyyy}", headerFont, XBrushes.Black, new XRect(margin, 40, graphics.PageSize.Width - 2 * margin, 15), XStringFormats.TopLeft);

//    // Draw the invoice items table header
//    tableTop = 55;
//    graphics.DrawRectangle(XPens.Black, margin, tableTop, tableWidth, rowHeight);
//    graphics.DrawString("Product", headerFont, XBrushes.Black, new XRect(margin, tableTop, 60, rowHeight), XStringFormats.TopLeft);
//    graphics.DrawString("Qty", headerFont, XBrushes.Black, new XRect(margin + 60, tableTop, 30, rowHeight), XStringFormats.TopCenter);
//    graphics.DrawString("Unit Price", headerFont, XBrushes.Black, new XRect(margin + 90, tableTop, 40, rowHeight), XStringFormats.TopCenter);
//    graphics.DrawString("Total", headerFont, XBrushes.Black, new XRect(margin + 130, tableTop, 40, rowHeight), XStringFormats.TopCenter);

//    // Draw the invoice items table rows
//    tableTop += rowHeight;
//    foreach (var item in invoice.InvoiceItems)
//    {
//        graphics.DrawRectangle(XPens.Black, margin, tableTop, tableWidth, rowHeight);
//        graphics.DrawString(item.Product.ProductName, smallFont, XBrushes.Black, new XRect(margin, tableTop, 60, rowHeight), XStringFormats.TopLeft);
//        graphics.DrawString(item.Quantity.ToString(), smallFont, XBrushes.Black, new XRect(margin + 60, tableTop, 30, rowHeight), XStringFormats.TopCenter);
//        graphics.DrawString($"{item.UnitPrice:C}", smallFont, XBrushes.Black, new XRect(margin + 90, tableTop, 40, rowHeight), XStringFormats.TopCenter);
//        graphics.DrawString($"{item.TotalPrice:C}", smallFont, XBrushes.Black, new XRect(margin + 130, tableTop, 40, rowHeight), XStringFormats.TopCenter);
//        tableTop += rowHeight;
//    }

//    // Draw the total amount at the bottom
//    graphics.DrawRectangle(XPens.Black, margin, tableTop, tableWidth, rowHeight);
//    graphics.DrawString($"Total Amount: {invoice.TotalAmount:C}", headerFont, XBrushes.Black, new XRect(margin, tableTop, tableWidth, rowHeight), XStringFormats.TopRight);
//}
//private void DrawInvoice(XGraphics graphics, Invoice invoice)
//{
//    var titleFont = new XFont("Verdana", 16, XFontStyle.Bold);
//    var headerFont = new XFont("Verdana", 12, XFontStyle.Bold);
//    var smallFont = new XFont("Verdana", 10, XFontStyle.Regular);

//    // Set up dimensions for A4 size page
//    int margin = 20;
//    int rowHeight = 20;
//    int tableTop = 40;
//    int tableWidth = (int)graphics.PageSize.Width - 2 * margin;

//    // Header
//    graphics.DrawString("Invoice Details", titleFont, XBrushes.Black, new XRect(0, 0, graphics.PageSize.Width, 40), XStringFormats.TopCenter);
//    graphics.DrawLine(XPens.Black, 0, 40, graphics.PageSize.Width, 40);

//    // Invoice Details
//    //graphics.DrawString($"Invoice Number: {invoice.InvoiceId}", headerFont, XBrushes.Black, new XRect(margin, 50, graphics.PageSize.Width - 2 * margin, 20), XStringFormats.TopLeft);
//    //graphics.DrawString($"Invoice Date: {invoice.InvoiceDate:dd-MM-yyyy}", headerFont, XBrushes.Black, new XRect(margin, 70, graphics.PageSize.Width - 2 * margin, 20), XStringFormats.TopLeft);

//    // Draw the invoice items table header
//    tableTop += 20;
//    graphics.DrawRectangle(XPens.Black, margin, tableTop, tableWidth, rowHeight);
//    graphics.DrawString("Product", headerFont, XBrushes.Black, new XRect(margin, tableTop, 200, rowHeight), XStringFormats.TopLeft);
//    graphics.DrawString("Quantity", headerFont, XBrushes.Black, new XRect(margin + 200, tableTop, 100, rowHeight), XStringFormats.TopCenter);
//    graphics.DrawString("Unit Price", headerFont, XBrushes.Black, new XRect(margin + 300, tableTop, 100, rowHeight), XStringFormats.TopCenter);
//    graphics.DrawString("Total Price", headerFont, XBrushes.Black, new XRect(margin + 400, tableTop, 100, rowHeight), XStringFormats.TopCenter);

//    // Draw the invoice items table rows
//    tableTop += rowHeight;
//    foreach (var item in invoice.InvoiceItems)
//    {
//        graphics.DrawRectangle(XPens.Black, margin, tableTop, tableWidth, rowHeight);
//        graphics.DrawString(item.Product.ProductName, smallFont, XBrushes.Black, new XRect(margin, tableTop, 200, rowHeight), XStringFormats.TopLeft);
//        graphics.DrawString(item.Quantity.ToString(), smallFont, XBrushes.Black, new XRect(margin + 200, tableTop, 100, rowHeight), XStringFormats.TopCenter);
//        graphics.DrawString($"{item.UnitPrice:C}", smallFont, XBrushes.Black, new XRect(margin + 300, tableTop, 100, rowHeight), XStringFormats.TopCenter);
//        graphics.DrawString($"{item.TotalPrice:C}", smallFont, XBrushes.Black, new XRect(margin + 400, tableTop, 100, rowHeight), XStringFormats.TopCenter);
//        tableTop += rowHeight;
//    }

//    // Draw the total amount at the bottom
//    graphics.DrawRectangle(XPens.Black, margin, tableTop, tableWidth, rowHeight);
//    graphics.DrawString($"Total Amount: {invoice.TotalAmount:C}", headerFont, XBrushes.Black, new XRect(margin, tableTop, tableWidth, rowHeight), XStringFormats.TopRight);
//}
//public IActionResult GeneratePdfAndPrint(int invoiceId)
//{
//    var invoice = GetInvoiceById(invoiceId);
//    var stream = new MemoryStream();

//    var document = new PdfDocument();
//    var page = document.AddPage();
//    page.Size = PdfSharpCore.PageSize.A4;
//    var graphics = XGraphics.FromPdfPage(page);

//    DrawInvoice(graphics, invoice);

//    document.Save(stream, false);
//    var pdfBytes = stream.ToArray();
//    var pdfUrl = Url.Action("GetPdf", new { invoiceId = invoiceId });

//    return Content($"<script>printInvoice('{pdfUrl}');</script>", "text/html");
//}
//public IActionResult GetPdf(int invoiceId)
//{
//    var invoice = GetInvoiceById(invoiceId);

//    using (var stream = new MemoryStream())
//    {
//        var document = new PdfDocument();
//        var page = document.AddPage();
//        page.Size = PdfSharpCore.PageSize.A4;
//        var graphics = XGraphics.FromPdfPage(page);

//        DrawInvoice(graphics, invoice);

//        document.Save(stream, false);
//        return File(stream.ToArray(), "application/pdf");
//    }
//}