// تعريف دالة لتحديث الجدول
function updateTable() {
    $.ajax({
        url: '/Product/LoadProductsPartial',
        type: 'GET',
        success: function (data) {
            $('#productsTableContainer').html(data);
        },
        error: function (xhr, status, error) {
            console.error('Error updating table:', error);
        }
    });
}

$(document).ready(function () {
    // تعريف دالة تأكيد الحذف
    function DeleteConfirm() {
        return new Promise(resolve => {
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to delete this Item?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, Delete It!',
                cancelButtonText: 'No, Cancel'
            }).then((result) => {
                resolve(result.isConfirmed); // التأكيد على إتمام عملية الحذف
            });
        });
    }

    // التعامل مع عملية الحذف عند تقديم النموذج
    $(document).on('submit', 'form.deleteForm', function (e) {
        e.preventDefault(); // منع الإرسال الافتراضي للنموذج
        var form = this;

        DeleteConfirm().then((isConfirmed) => {
            if (isConfirmed) {
                $.ajax({
                    url: $(form).attr('action'),
                    type: 'POST',
                    data: $(form).serialize(),
                    success: function () {
                        Swal.fire(
                            'Deleted!',
                            'Your item has been deleted.',
                            'success'
                        );
                        updateTable(); // تحديث الجدول بعد الحذف
                    },
                    error: function () {
                        Swal.fire(
                            'Error!',
                            'There was an error deleting the item.',
                            'error'
                        );
                    }
                });
            }
        });
    });
});
