$(document).ready(function () {
    loadMemberAndCustomerLineChart()
});

function loadMemberAndCustomerLineChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetMemberAndBookingLineChartData",
        type: 'GET',
        dataType: 'json',
        success: function (data) {

            loadLineChart("newMembersAndBookingLineChart", data);
            $(".chart-spinner").hide();
        }
    })
}

function loadLineChart(id, data) {
    var chartColors = getChartColorsArray(id);
    var options = {
        colors: chartColors,
        series: data.series,
        chart: {
            height: 350,
            type: 'line',
            zoom: {
                enabled: false
            },
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            width: [5, 7, 5],
            curve: 'smooth',
            dashArray: [0, 8, 5]
        },
        title: {
            text: '',
            align: 'left'
        },
        legend: {
            tooltipHoverFormatter: function (val, opts) {
                return val + ' - <strong>' + opts.w.globals.series[opts.seriesIndex][opts.dataPointIndex] + '</strong>'
            },
            labels: {
                colors:"#fff",
            },
        },
        markers: {
            size: 4,
            hover: {
                sizeOffset: 6
            }
        },
        xaxis: {
            categories: data.categories,
            labels: {
                style: {
                    colors:"#fff",
                },
            },
        },
        tooltip: {
            y: [
                {
                    title: {
                        formatter: function (val) {
                            return val
                        }
                    }
                },
                {
                    title: {
                        formatter: function (val) {
                            return val + " (per person)"
                        }
                    }
                }
            ]
        },
        grid: {
            borderColor: '#f1f1f1',
        },
        tooltip: {
            theme: 'dark'
        },
        yaxis: {
            labels: {
                style: {
                    colors: "#fff",
                },
            },
        },
    };
    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}
