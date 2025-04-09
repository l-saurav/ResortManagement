var dataTable;

$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status');
    loadDataTable(status);
});

function loadDataTable(status) {
    dataTable = $('#tblBookings').DataTable({
        "ajax": {
            url: '/Booking/GetAll?status='+status
        },
        "columns": [
            { data: 'id', "width": '5%' },
            { data: 'name', "width": '15%' }, // Removed extra spaces
            { data: 'phoneNumber', "width": '15%' },
            { data: 'email', "width": '20%' },
            { data: 'status', "width": '10%' },
            { data: 'checkInDate', "width": '10%' },
            { data: 'nights', "width": '10%' },
            { data: 'totalCost',render: $.fn.dataTable.render.number(',','.',2), "width": '15%' },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group">
                        <a href="/Booking/BookingDetails?bookingId=${data}" class="btn btn-outline-warning mx-2">
                            <i class="bi bi-pencil-square"></i> Details
                        </a>
                    </div>`
                }
            }
        ]
    });
}