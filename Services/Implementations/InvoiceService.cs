using InvoiceManagementSystem.Models;
using InvoiceManagementSystem.Repositories.Interfaces;
using InvoiceManagementSystem.Services.Interfaces;
using InvoiceManagementSystem.ViewMoldel;
using Microsoft.Extensions.Caching.Memory;
using QRCoder;
using System.Linq.Expressions;

namespace InvoiceManagementSystem.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public InvoiceService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<(IEnumerable<Invoice> Invoices, bool HasMore)> GetInvoicesAsync(int pageIndex, int pageSize)
        {

            // استخدم مفتاح فريد يعتمد على pageIndex و pageSize
            string cacheKey = $"invoiceList_{pageIndex}_{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Invoice> invoices))
            {
                invoices = await _unitOfWork.Invoices.GetInvoicesAsync(pageIndex, pageSize);

                if (invoices is null)
                    return default;

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, invoices, cacheOptions);
            }

            // تحقق مما إذا كانت هناك بيانات إضافية
            bool hasMore = invoices.Count() == pageSize; // إذا كانت البيانات المسترجعة أقل من pageSize، فهذا يعني أنه لا توجد بيانات إضافية
            return (invoices, hasMore);

        }
        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            if (id <= 0)
                return default;

            var invoice = await _unitOfWork.Invoices.GetInvoiceByIdAsync(id);

            if (invoice is null)
                return default;

            return invoice;
        }
        public async Task<Invoice> CreateInvoiceAsync(InvoiceViewModel model)
        {
            if (model is null)
                return default;

            var invoice = ConvertToModel(model);

            var addedInvoince = await _unitOfWork.Invoices.CreateInvoiceAsync(invoice);
            await _unitOfWork.CompleteAsync();
            _cache.Remove("invoiceList");

            return addedInvoince;
        }
        public async Task<Invoice> UpdateInvoiceAsync(InvoiceViewModel model)
        {
            if (model is null)
                return default;

            var existingInvoice = await _unitOfWork.Invoices.GetInvoiceByIdAsync(model.InvoiceId);
            if (existingInvoice is null)
                return default;


            existingInvoice.InvoiceDate = model.InvoiceDate;
            existingInvoice.TotalAmount = model.TotalAmount;

            foreach (var item in model.InvoiceItems)
            {
                var exitingItem = existingInvoice.InvoiceItems.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (exitingItem != null)
                {
                    //تحديث العناصر
                    exitingItem.Quantity = item.Quantity;
                    exitingItem.UnitPrice = item.UnitPrice;
                    exitingItem.TotalPrice = item.TotalPrice;
                }
                else
                {
                    //إضافة عنصر جديد
                    existingInvoice.InvoiceItems.Add(new InvoiceItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }
            }

            //حذف العناصر التي لم تعد موجودة في النموذج
            var itemToDelete = existingInvoice.InvoiceItems
           .Where(a => !model.InvoiceItems.Any(mi => mi.ProductId == a.ProductId)).ToList();

            foreach (var item in itemToDelete)
            {
                existingInvoice.InvoiceItems.Remove(item);
            }

            var editedInvoice = _unitOfWork.Invoices.UpdateInvoiceAsync(existingInvoice);
            await _unitOfWork.CompleteAsync();
            _cache.Remove("invoiceList");

            return editedInvoice;
        }
        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            if (id <= 0)
                return default;

            await _unitOfWork.Invoices.DeleteInvoiceAsync(id);
            await _unitOfWork.CompleteAsync();
            _cache.Remove("invoiceList");

            return true;
        }


        public async Task<int> CountAsync(Expression<Func<Invoice, bool>> match)
        {
            if (match is null)
                return default;

            return await _unitOfWork.Invoices.CountAsync(match);
        }

        public async Task<int> CountAsync()
        {
            return await _unitOfWork.Invoices.CountAsync();
        }

        public async Task<InvoiceViewModel> PrintInvoiceAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.Invoices.GetInvoiceByIdAsync(invoiceId);

            string qrText = $"http://localhost:5170/Invoice/Details?invoiceId/{invoiceId}";
            string qrCodeImage = CreateQRCode(qrText);

            var invoiceViewModel = ConvertToViewModel(invoice, qrCodeImage);
            return invoiceViewModel;
        }

        private static Invoice ConvertToModel(InvoiceViewModel model)
        {
            return new Invoice
            {
                InvoiceDate = model.InvoiceDate,
                TotalAmount = model.TotalAmount,
                InvoiceItems = model.InvoiceItems.Select(i => new InvoiceItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };
        }

        //private Invoice GetInvoiceById(int id)
        //{
        //    if (id <= 0)
        //        return default;

        //    var invoice = _context.Invoices.Include(i => i.InvoiceItems)
        //                .ThenInclude(s => s.Product).FirstOrDefault(p => p.InvoiceId == id);

        //    if (invoice is null)
        //        return default;

        //    return invoice;
        //}

        private string CreateQRCode(string qrText)
        {
            if (string.IsNullOrEmpty(qrText))
                return null;

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))

            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                string base64Image = Convert.ToBase64String(qrCodeImage);
                return string.Format("data:image/png;base64,{0}", base64Image);
            }
        }

        private static InvoiceViewModel ConvertToViewModel(Invoice invoice, string qrCodeImage)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            return new InvoiceViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceDate = invoice.InvoiceDate,
                InvoiceItems = invoice.InvoiceItems?.Select(i => new InvoiceItemViewModel
                {
                    ProductName = i.Product?.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                }).ToList(),
                QRCodeImage = qrCodeImage,
            };
        }



        //public async InvoiceViewModel ConvertInvoiceToInvoiceViewModel(Invoice invoice)
        //{
        //    return new InvoiceViewModel
        //    {
        //        InvoiceId = invoice.InvoiceId,
        //        InvoiceDate = invoice.InvoiceDate,
        //        InvoiceItems = invoice.InvoiceItems.Select(i => new InvoiceItemViewModel
        //        {
        //            ProductId = i.ProductId,
        //            Quantity = i.Quantity,
        //            UnitPrice = i.UnitPrice,

        //        }).ToList(),

        //        Products =await _unitOfWork.Products.GetProductSelectListItemsAsync()
        //    };
        //}


    }
}
