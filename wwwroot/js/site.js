
//$(document).ready(function () {

//    // دالة تحديث الجدول بعد إضافة أو تعديل الفاتورة
//    function updateTable() {
//        $.ajax({
//            url: '/Invoice/LoadInvoicesPartial'
//            type: 'GET',
//            success: function (data) {
//                $('#invoicesTableContainer').html(data);
//            },
//            error: function (xhr, status, error) {
//                console.error('Error updating table:', error);
//            }
//        });
//    }

//    //الاضافـة
//    //===================================================================================//


//    $('#createInvoiceModal').on('show.bs.modal', function () {
//        $.ajax({
//            url: '@Url.Action("Create", "Invoice")',
//            type: 'GET',
//            success: function (data) {
//                $('#createInvoiceContent').html(data);
//            },
//            error: function () {
//                console.error("Failed to load the create invoice form.");
//            }
//        });
//    });


//    $(document).on('submit', '#createInvoiceForm', function (e) {
//        e.preventDefault();

//        $.ajax({
//            url: $(this).attr('action'),
//            type: 'POST',
//            data: $(this).serialize(),
//            success: function (response) {
//                if (response.success) {
//                    $('#createInvoiceModal').modal('hide');
//                    updateTable();
//                } else {
//                    alert(response.errorMessage);
//                }
//            },
//            error: function (xhr, status, error) {
//                console.error('Error during form submission: ', error);
//                alert('Error during form submission. Please check the console for details.');
//            }
//        });

//        // التأكد من إزالة كافة التأثيرات بشكل إضافي في حال بقاء الجمود
//        $(document).on('hidden.bs.modal', function () {
//            $('.modal-backdrop').remove();
//            $('body').removeClass('modal-open');
//            $('body').css('overflow', '');
//            $('body').css('padding-right', '');
//        });
//    });


//    //التعديـــــــل
//    //===================================================================================//


//    $(document).on('click', '.editInvoiceBtn', function () {
//        var invoiceId = $(this).data('id');

//        $.ajax({
//            url: '@Url.Action("Edit", "Invoice")',
//            type: 'GET',
//            data: { id: invoiceId },
//            success: function (data) {
//                $('#modalContent').html(data);
//                $('#invoiceModal').modal('show');
//            },
//            error: function (xhr, status, error) {
//                console.error('Error loading form: ', error);
//            }
//        });
//    });


//    $(document).on('submit', '#editInvoiceForm', function (e) {
//        e.preventDefault();

//        $.ajax({
//            url: $(this).attr('action'),
//            type: 'POST',
//            data: $(this).serialize(),
//            success: function (response) {
//                if (response.success) {
//                    $('#invoiceModal').modal('hide');
//                    updateTable();
//                } else {
//                    alert(response.errorMessage);
//                }
//            },
//            error: function (xhr, status, error) {
//                console.error('Error during form submission: ', error);
//                alert('Error during form submission. Please check the console for details.');
//            }
//        });
//    });

//    setTimeout(function () {
//        updateTable();
//    }, 500);

//});













//// Parse the JSON products from the ViewBag
//var products = JSON.parse('@Html.Raw(ViewBag.ProductsJson)');

//function addRow() {
//    var table = document.getElementById("invoice-items-table").getElementsByTagName('tbody')[0];
//    var newRow = table.insertRow(table.rows.length);

//    // Create the product select dropdown
//    var productCell = newRow.insertCell(0);
//    var productSelect = document.createElement("select");
//    productSelect.classList.add("form-control");
//    productSelect.name = "InvoiceItems[" + (table.rows.length - 1) + "].ProductId";

//    // Populate the select dropdown with product options
//    products.forEach(function (product) {
//        var option = document.createElement("option");
//        option.value = product.ProductId;
//        option.text = product.ProductName;
//        productSelect.appendChild(option);
//    });

//    productCell.appendChild(productSelect);

//    // Create quantity input
//    var quantityCell = newRow.insertCell(1);
//    quantityCell.innerHTML = '<input type="number" name="InvoiceItems[' + (table.rows.length - 1) + '].Quantity" class="form-control" placeholder="Quantity" oninput="updateTotalPrice(this)" />';

//    // Create unit price input
//    var unitPriceCell = newRow.insertCell(2);
//    unitPriceCell.innerHTML = '<input type="number" step="0.01" name="InvoiceItems[' + (table.rows.length - 1) + '].UnitPrice" class="form-control" placeholder="Unit Price" oninput="updateTotalPrice(this)" />';

//    // Create total price display
//    var totalPriceCell = newRow.insertCell(3);
//    totalPriceCell.innerHTML = '<span class="total-price">0.00</span>';

//    // Create action button
//    var actionCell = newRow.insertCell(4);
//    actionCell.innerHTML = '<button type="button" onclick="removeRow(this)" class="btn btn-danger">Remove</button>';
//}

//function removeRow(button) {
//    var row = button.parentNode.parentNode;
//    row.parentNode.removeChild(row);
//    updateAllIndexes();
//    updateAllTotalPrices();
//    updateTotalAmount();
//}

//function updateTotalPrice(element) {
//    var row = element.closest('tr');
//    var quantity = row.querySelector('input[name$=".Quantity"]').value;
//    var unitPrice = row.querySelector('input[name$=".UnitPrice"]').value;
//    var totalPrice = (quantity * unitPrice).toFixed(2);
//    row.querySelector('.total-price').innerText = totalPrice;
//    updateTotalAmount();
//}

//function updateTotalAmount() {
//    var totalAmount = 0;
//    var rows = document.querySelectorAll('#invoice-items-table tbody tr');
//    rows.forEach(function (row) {
//        var totalPrice = parseFloat(row.querySelector('.total-price').innerText);
//        totalAmount += totalPrice;
//    });
//    document.getElementById("total-amount").innerText = totalAmount.toFixed(2);
//}














//function updateTable() {
//    $.ajax({
//        url: '/Invoice/LoadInvoicesPartial', // الأكشن الذي يعيد البيانات
//        type: 'GET',
//        success: function (data) {
//            $('#invoicesTableContainer').html(data); // تحديث محتوى الجدول
//        }
//    });
//}


//// عند الضغط على زر التعديل
//$(document).on('click', '.editInvoiceBtn', function () {
//    var invoiceId = $(this).data('id');

//    // إرسال طلب AJAX لجلب النموذج الخاص بالتعديل
//    $.ajax({
//        url: '@Url.Action("Edit", "Invoice")',  // استدعاء الـ Action الذي يعرض النموذج
//        type: 'GET',
//        data: { id: invoiceId },
//        success: function (data) {
//            // عرض النموذج داخل الـ Modal
//            $('#modalContent').html(data);
//            $('#invoiceModal').modal('show');
//        },
//        error: function (xhr, status, error) {
//            console.error('Error loading form: ', error);
//        }
//    });
//});

//// عند إرسال النموذج
//$(document).on('submit', '#editInvoiceForm', function (e) {
//    e.preventDefault();

//    // إرسال النموذج باستخدام AJAX
//    $.ajax({
//        url: $(this).attr('action'),  // رابط الـ Action
//        type: 'POST',
//        data: $(this).serialize(),    // إرسال البيانات
//        success: function (response) {
//            if (response.success) {
//                $('#invoiceModal').modal('hide');  // إغلاق الـ Modal بعد النجاح
//                updateTable();  // تحديث الجدول بعد التعديل
//            } else {
//                alert(response.errorMessage);  // عرض خطأ في حال الفشل
//            }
//        },
//        error: function (xhr, status, error) {
//            console.error('Error during form submission: ', error);
//        }
//    });
//});