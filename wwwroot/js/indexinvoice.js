$(document).ready(function () {

    // دالة تحديث الجدول بعد إضافة أو تعديل الفاتورة
    function updateTable() {
        $.ajax({
            url: '/Invoice/LoadInvoicesPartial',
            type: 'GET',
            success: function (data) {
                $('#invoicesTableContainer').html(data);
            },
            error: function (xhr, status, error) {
                console.error('Error updating table:', error);
            }
        });
    }


    // إعادة تهيئة زر إضافة العناصر
    function reinitializeAddRowButton() {
        $('#addRowButton').off('click').on('click', function () {
            addRow();
        });
    }

    // إعادة تعيين النماذج وتفريغ المحتوى عند إغلاق الشاشة
    $('#createInvoiceModal, #editInvoiceModal').on('hidden.bs.modal', function () {
        $(this).find('form')[0].reset(); // إعادة تعيين النماذج
        $('#createInvoiceContent, #editModalContent').html(''); // تفريغ المحتوى
        $('.modal-backdrop').remove(); // إزالة الخلفية
        $('body').removeClass('modal-open'); // إزالة class modal-open
        $('body').css('overflow', ''); // إعادة ضبط overflow
        $('body').css('padding-right', ''); // إعادة ضبط padding-right
    });



    //الاضافـة
    //===================================================================================//


    $('#createInvoiceModal').on('show.bs.modal', function () {
        $.ajax({
            url: '@Url.Action("Create", "Invoice")',
            type: 'GET',
            success: function (data) {
                $('#createInvoiceContent').html(data);
                // إعادة تهيئة زر إضافة الصفوف عند فتح النافذة
                reinitializeAddRowButton();
            },
            error: function () {
                console.error("Failed to load the create invoice form.");
            }
        });
    });


    $(document).on('submit', '#createInvoiceForm', function (e) {
        e.preventDefault();

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    $('#createInvoiceModal').modal('hide');
                    if (response.message) {
                        toastr.success(response.message); // عرض رسالة النجاح
                    }
                    printInvoiceSmall(response.invoiceId);
                    updateTable();
                } else {
                    alert(response.errorMessage);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error during form submission: ', error);
                alert('Error during form submission. Please check the console for details.');
            }
        });

        // التأكد من إزالة كافة التأثيرات بشكل إضافي في حال بقاء الجمود
        $(document).on('hidden.bs.modal', function () {
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('body').css('overflow', '');
            $('body').css('padding-right', '');
        });
    });






    //التعديـــــــل
    //===================================================================================//


    $(document).on('click', '.editInvoiceBtn', function () {
        var invoiceId = $(this).data('id');

        $.ajax({
            url: '@Url.Action("Edit", "Invoice")',
            type: 'GET',
            data: { id: invoiceId },
            success: function (data) {
                $('#editModalContent').html(data);
                $('#editInvoiceModal').modal('show');
                // إعادة تهيئة زر إضافة الصفوف عند فتح النافذة
                reinitializeAddRowButton();
            },
            error: function (xhr, status, error) {
                console.error('Error loading form: ', error);
            }
        });
    });


    $(document).on('submit', '#editInvoiceForm', function (e) {
        e.preventDefault();

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    $('#editInvoiceModal').modal('hide');
                    if (response.message) {
                        toastr.success(response.message); // عرض رسالة النجاح
                    }
                    updateTable();
                } else {
                    alert(response.errorMessage);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error during form submission: ', error);
                alert('Error during form submission. Please check the console for details.');
            }
        });
    });


    //Details Invoice
    //===================================================================================//


    $(document).on('click', '.detailsInvoiceBtn', function () {
        var invoiceId = $(this).data('id');

        $.ajax({
            url: '@Url.Action("Details", "Invoice")',
            type: 'GET',
            data: { invoiceId: invoiceId },
            success: function (data) {
                $('#detailsModalContent').html(data);
                $('#detailsInvoiceModal').modal('show');
            },
            error: function (xhr, status, error) {
                console.error('Error loading form: ', error);
            }
        });
    });


    //Print  Invoice size A4
    //===================================================================================//

    // عند الضغط على زر الطباعة
    $(document).on('click', '#printInvoiceBtn', function () {
        printInvoiceA4();
    });

    function printInvoiceA4() {
        var printContent = document.getElementById('detailsModalContent').innerHTML;

        var printWindow = window.open('', '_blank');
        printWindow.document.write('<link rel="stylesheet" href="/css/print-invoice-A4.css" />');
        printWindow.document.write(printContent);
        printWindow.document.close();
        printWindow.focus();

        printWindow.onload = function () {
            printWindow.print();
            printWindow.close();
        };

        $('#detailsModalContent script').remove();
    }



    //Print Invoice size Small
    //===================================================================================//
    // عند الضغط على زر الطباعة
    $(document).on('click', '#printInvoicSmalleBtn', function () {
        var invoiceId = $(this).data('id');
        printInvoiceSmall(invoiceId)
    });

    function printInvoiceSmall(invoiceId) {
        $.ajax({
            url: '/Invoice/PrintInvoiceSmall/' + invoiceId,
            type: 'GET',
            success: function (invoiceHtml) {
                var printWindow = window.open('', '_blank');
                printWindow.document.write(invoiceHtml);
                printWindow.document.close();

                printWindow.onload = function () {
                    printWindow.focus();
                    printWindow.print();
                    printWindow.close();
                };

                window.onafterprint = function () {
                    updateTable();
                };
            },
            error: function () {
                toastr.error('Failed to load invoice for printing.');
            }
        });
    }

    setTimeout(function () {
        updateTable();
    }, 500);

});






// Parse the JSON products from the ViewBag
var products = JSON.parse('@Html.Raw(ViewBag.ProductsJson)');

function addRow() {
    var table = document.getElementById("invoice-items-table").getElementsByTagName('tbody')[0];
    var newRow = table.insertRow(table.rows.length);

    // Create the product select dropdown
    var productCell = newRow.insertCell(0);
    var productSelect = document.createElement("select");
    productSelect.classList.add("form-control");
    productSelect.name = "InvoiceItems[" + (table.rows.length - 1) + "].ProductId";

    // Populate the select dropdown with product options
    products.forEach(function (product) {
        var option = document.createElement("option");
        option.value = product.Value;  // استخدم 'Value' بدلاً من 'ProductId'
        option.text = product.Text;    // استخدم 'Text' بدلاً من 'ProductName'
        productSelect.appendChild(option);
    });


    productCell.appendChild(productSelect);
    console.log('productSelect: ', productSelect);
    // Create quantity input
    var quantityCell = newRow.insertCell(1);
    quantityCell.innerHTML = '<input type="number" name="InvoiceItems[' + (table.rows.length - 1) + '].Quantity" class="form-control" placeholder="Quantity" oninput="updateTotalPrice(this)" />';

    // Create unit price input
    var unitPriceCell = newRow.insertCell(2);
    unitPriceCell.innerHTML = '<input type="number" step="0.01" name="InvoiceItems[' + (table.rows.length - 1) + '].UnitPrice" class="form-control" placeholder="Unit Price" oninput="updateTotalPrice(this)" />';

    // Create total price display
    var totalPriceCell = newRow.insertCell(3);
    totalPriceCell.innerHTML = '<span class="total-price">0.00</span>';

    // Create action button
    var actionCell = newRow.insertCell(4);
    actionCell.innerHTML = '<button type="button" onclick="removeRow(this)" class="btn btn-danger">Remove</button>';
}

function removeRow(button) {
    var row = button.parentNode.parentNode;
    row.parentNode.removeChild(row);
    updateAllIndexes();
    updateAllTotalPrices();
    updateTotalAmount();
}

function updateTotalPrice(element) {
    var row = element.closest('tr');
    var quantity = row.querySelector('input[name$=".Quantity"]').value;
    var unitPrice = row.querySelector('input[name$=".UnitPrice"]').value;
    var totalPrice = (quantity * unitPrice).toFixed(2);
    row.querySelector('.total-price').innerText = totalPrice;
    updateTotalAmount();
}

function updateTotalAmount() {
    var totalAmount = 0;
    var rows = document.querySelectorAll('#invoice-items-table tbody tr');

    rows.forEach(function (row) {
        var totalPriceElement = row.querySelector('.total-price');

        // تحقق من وجود العنصر قبل الوصول إلى خاصيته
        if (totalPriceElement) {
            var totalPrice = parseFloat(totalPriceElement.innerText);
            totalAmount += totalPrice;
        }
    });

    var totalAmountElement = document.getElementById("total-amount");
    if (totalAmountElement) {
        totalAmountElement.innerText = totalAmount.toFixed(2);
    }
}
