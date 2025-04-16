$(document).ready(function () {
    loadCustomerBookingPieChart()
});

function loadCustomerBookingPieChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetBookingPieChartData",
        type: 'GET',
        dataType: 'json',
        success: function (data) {

            loadPieChart("customerBookingsPieChart", data);
            $(".chart-spinner").hide();
        }
    })
}

function loadPieChart(id, data) {
    var chartColors = getChartColorsArray(id);
    var options = {
        series: data.series,
        chart: {
            width: 380,
            type: 'pie',
        },
        labels: data.labels,
        legend: {
            position: 'bottom', // Position the legend below the chart
            horizontalAlign: 'center', // Center-align the legend
            labels: {
                colors: Array(data.labels.length).fill('#FFFFFF'), // Set all label text colors to white
            },
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: 200
                },
                stroke: {
                    show: true
                },
                legend: {
                    position: 'bottom',
                    horizontalAlign: 'center',
                    labels: {
                        colors: Array(data.labels.length).fill('#FFFFFF'), // Ensure white text for smaller screens
                    },
                },
            },
        }]
    };
    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}
