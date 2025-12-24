using ClosedXML.Excel;
using InvoiceManagementSystem.Data;
using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.ViewMoldel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InvoiceManagementSystem.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<ReportViewMoldel> GetDailyInvoicesReport()
        {
            var dailyInvoices = _context.Invoices
               .Include(i => i.InvoiceItems)
               .ThenInclude(p => p.Product)
               .Where(dt => dt.InvoiceDate.Date == DateTime.Today)
               .OrderBy(d => d.InvoiceDate)
               .AsNoTracking()
               .ToList();

            if (dailyInvoices is null)
                return default;

            var reportViewModelList = GenerateReportViewModelList(dailyInvoices);
            return reportViewModelList;
        }
        public IActionResult Index()
        {

            var reportsDaily = GetDailyInvoicesReport();
            return View(reportsDaily);
        }

        //public IActionResult LoadReportsPartial()
        //{
        //    var reportViewModelList = GetDailyInvoicesReport();
        //    return PartialView("_ReportTablePartial", reportViewModelList);
        //}

        public List<Invoice> GetInvoicesByStartDateAndEndDate(DateTime startDate, DateTime endDate)
        {
            // التحقق من صحة التواريخ
            if (startDate == default || endDate == default)
            {
                return default;
            }

            return _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(p => p.Product)
                .Where(dt =>
                dt.InvoiceDate.Date >= startDate.Date
                && dt.InvoiceDate.Date <= endDate.Date)
                .OrderBy(d => d.InvoiceDate)
                .AsNoTracking().ToList();
        }
        public IActionResult GetInvoicesReport(DateTime startDate, DateTime endDate)
        {
            var invoices = GetInvoicesByStartDateAndEndDate(startDate, endDate);
            if (invoices is null)
                return BadRequest();

            var reportViewModelList = GenerateReportViewModelList(invoices);
            return PartialView("_ReportTablePartial", reportViewModelList);
        }

        public IActionResult DailyInvoicesReport()
        {
            ViewBag.ReportType = "daily";
            var reportViewModelList = GetDailyInvoicesReport();

            return PartialView("_ReportTablePartial", reportViewModelList);
        }
        public IActionResult WeeklyInvoicesReport()
        {
            ViewBag.ReportType = "weekly";

            var weeklyInvoices = _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(p => p.Product)
                .Where(dt => dt.InvoiceDate.Date >= DateTime.Today.AddDays(-7) && dt.InvoiceDate.Date <= DateTime.Today)
                .OrderBy(d => d.InvoiceDate)
                .AsNoTracking().ToList();

            if (weeklyInvoices is null)
                return BadRequest();

            var reportViewModelList = GenerateReportViewModelList(weeklyInvoices);
            return PartialView("_ReportTablePartial", reportViewModelList);
        }
        public IActionResult MonthlyInvoicesReport()
        {
            ViewBag.ReportType = "monthly";

            var monthlyInvoices = _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(p => p.Product)
                .Where(dt => dt.InvoiceDate.Month == DateTime.Today.Month && dt.InvoiceDate.Year == DateTime.Today.Year)
                .OrderBy(d => d.InvoiceDate)
                .AsNoTracking().ToList();

            if (monthlyInvoices is null)
                return BadRequest();

            var reportViewModelList = GenerateReportViewModelList(monthlyInvoices);
            return PartialView("_ReportTablePartial", reportViewModelList);
        }

        public IActionResult ExportToExcel(DateTime startDate, DateTime endDate)
        {

            if (startDate == default || endDate == default)
            {
                return BadRequest("Invalid dates provided.");
            }

            var reportDatas = GetInvoicesByStartDateAndEndDate(startDate, endDate);
            var reportViewModelList = GenerateReportViewModelList(reportDatas);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");

                // إضافة رؤوس الأعمدة
                worksheet.Cell(1, 1).Value = "Invoice Date";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "Unit Price";
                worksheet.Cell(1, 4).Value = "Quantity";
                worksheet.Cell(1, 5).Value = "Total Price";
                // worksheet.Cell(1, 6).Value = "Total Amount";

                int row = 2;
                foreach (var invoice in reportViewModelList)
                {
                    worksheet.Cell(row, 1).Value = invoice.InvoiceDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 2).Value = invoice.ProductName;
                    worksheet.Cell(row, 3).Value = invoice.UnitPrice;
                    worksheet.Cell(row, 4).Value = invoice.Quantity;
                    worksheet.Cell(row, 5).Value = invoice.TotalPrice;

                    row++;
                }

                string fileName = $"Report_{Guid.NewGuid().ToString().Substring(0, 4)}.xlsx";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        public IActionResult ExportToPDF(DateTime startDate, DateTime endDate)
        {
            if (startDate == default || endDate == default)
            {
                return BadRequest("Invalid dates provided.");
            }

            var reportDatas = GetInvoicesByStartDateAndEndDate(startDate, endDate);
            var reportViewModelList = GenerateReportViewModelList(reportDatas);

            using (var stream = new MemoryStream())
            {
                var document = new Document();
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // إضافة عنوان التقرير
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("Invoice Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // 2. إضافة التاريخ الحالي
                var currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm"); // تاريخ ووقت بصيغة مخصصة
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                var dateParagraph = new Paragraph($"Report Date: {currentDate}", dateFont);
                dateParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(dateParagraph);

                // 3. إضافة نوع التقرير
                var reportTypeFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                //// var reportTypeParagraph = new Paragraph($"Report Type: {GetReportTypeDisplay(reportType)}", reportTypeFont);
                // reportTypeParagraph.Alignment = Element.ALIGN_LEFT;
                // document.Add(reportTypeParagraph);

                // إضافة خط فارغ
                document.Add(new Paragraph("\n"));

                // إنشاء جدول PDF بـ 6 أعمدة
                PdfPTable table = new PdfPTable(5); // عدد الأعمدة
                table.WidthPercentage = 100;

                // تغيير لون خلفية الخلايا إلى الأزرق الفاتح
                BaseColor lightBlue = new BaseColor(173, 216, 230); // اللون الأزرق الفاتح

                // إضافة رؤوس الأعمدة مع تلوين
                PdfPCell cell = new PdfPCell(new Phrase("Invoice Date"));
                cell.BackgroundColor = lightBlue; // تغيير اللون إلى الأزرق الفاتح
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Product Name"));
                cell.BackgroundColor = lightBlue;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Unit Price"));
                cell.BackgroundColor = lightBlue;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Quantity"));
                cell.BackgroundColor = lightBlue;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Total Price"));
                cell.BackgroundColor = lightBlue;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);

                //cell = new PdfPCell(new Phrase("Total Amount"));
                //cell.BackgroundColor = lightBlue;
                //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                //table.AddCell(cell);

                // إضافة الصفوف
                foreach (var invoice in reportViewModelList)
                {
                    table.AddCell(new PdfPCell(new Phrase(invoice.InvoiceDate.ToString("yyyy-MM-dd"))));
                    table.AddCell(new PdfPCell(new Phrase(invoice.ProductName)));
                    table.AddCell(new PdfPCell(new Phrase(invoice.UnitPrice.ToString())));
                    table.AddCell(new PdfPCell(new Phrase(invoice.Quantity.ToString())));
                    table.AddCell(new PdfPCell(new Phrase(invoice.TotalPrice.ToString())));
                    // table.AddCell(new PdfPCell(new Phrase(invoice.TotalAmount.ToString())));
                }

                document.Add(table);
                document.Close();

                // حفظ الملف باستخدام جزء من الـ GUID لاسم مميز
                string fileName = $"Report_{Guid.NewGuid().ToString().Substring(0, 4)}.pdf";
                var content = stream.ToArray();
                return File(content, "application/pdf", fileName);
            }
        }

        private List<Invoice> GetFilteredInvoices(string reportType)
        {
            if (string.IsNullOrEmpty(reportType))
                return default;

            IQueryable<Invoice> invoices = _context.Invoices.Include(i => i.InvoiceItems)
                .ThenInclude(p => p.Product);

            // فلترة الفواتير بناءً على نوع التقرير
            switch (reportType)
            {
                case "daily":
                    invoices = invoices.Where(dt =>
                    dt.InvoiceDate.Date == DateTime.Today);
                    break;
                case "weekly":
                    invoices = invoices.Where(dt =>
                    dt.InvoiceDate.Date >= DateTime.Today.AddDays(-7)
                    && dt.InvoiceDate.Date <= DateTime.Today);
                    break;
                case "monthly":
                    invoices = invoices.Where(dt =>
                    dt.InvoiceDate.Month == DateTime.Today.Month
                    && dt.InvoiceDate.Year == DateTime.Today.Year);
                    break;
                default:
                    invoices = invoices.Take(0);
                    break;
            }

            return invoices.OrderBy(d => d.InvoiceDate).ToList();
        }
        // دالة للحصول على نوع التقرير بشكل مقروء
        private string GetReportTypeDisplay(string reportType)
        {
            if (string.IsNullOrEmpty(reportType))
                return default;

            switch (reportType.ToLower())
            {
                case "daily":
                    return "Daily Report";
                case "weekly":
                    return "Weekly Report";
                case "monthly":
                    return "Monthly Report";
                default:
                    return "Custom Report";
            }
        }
        private List<ReportViewMoldel> GenerateReportViewModelList(List<Invoice> invoices)
        {
            var reportViewModelList = new List<ReportViewMoldel>();

            foreach (var item in invoices)
            {
                foreach (var x in item.InvoiceItems)
                {
                    var reportViewMoldel = new ReportViewMoldel
                    {
                        InvoiceId = item.InvoiceId,
                        InvoiceDate = item.InvoiceDate,
                        ProductName = x.Product?.ProductName ?? "Unknown", // معالجة إذا كان المنتج غير موجود
                        UnitPrice = x.UnitPrice,
                        Quantity = x.Quantity,
                        TotalAmount = item.InvoiceItems.Sum(i => i.TotalPrice)
                    };

                    reportViewModelList.Add(reportViewMoldel);
                }
            }

            return reportViewModelList;
        }



    }


}

