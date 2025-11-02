const MONTHS = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
    'August',
    'September',
    'October',
    'November',
    'December'
];

var SnitzCharts = SnitzCharts ||
{
    GetTopicsByUser() {
        $.ajax({
            type: "POST",
            url: SnitzVars.baseUrl + "/Charts/TopicsByUser",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (chData) {
                document.getElementById('spinner_topic').style.display = 'none';
                document.getElementById('chart_topic').style.display = 'block';
                var aData = chData;
                var aLabels = aData[0];
                var aDatasets1 = aData[1];

                var ctx = $("#chart_topic");
                var topicChart = new Chart(ctx,
                {
                    type: 'bar',
                    autoSkip: false,
                    animation: false,
                    showDataPoints: true,
                    responsive: true,
                    data: {
                        labels: aLabels,
                        datasets: [
                            {
                                label: "Topics started",
                                data: aDatasets1,
                                backgroundColor: 'blue'
                            }
                        ]
                    },
                    options: {
                        scaleShowValues: true,
                        scales: {
                            y: { ticks: { beginAtZero: true } },
                            x: { ticks: { autoSkip: false } }
                        }
                    }
                });
            }
        });
    }
    , GetReplyData() {
        $.ajax({
            type: "POST",
            url: SnitzVars.baseUrl + "/Charts/RepliesByUser",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (chData) {
                document.getElementById('spinner_reply').style.display = 'none';
                document.getElementById('chart_reply').style.display = 'block';
                var aData = chData;
                var aLabels = aData[0];
                var aDatasets1 = aData[1];

                var ctx = $("#chart_reply");
                var replyChart = new Chart(ctx, {
                    type: 'bar',
                    autoSkip: false,
                    animation: false,
                    showDataPoints: true,
                    responsive: true,
                    data: {
                        labels: aLabels,
                        datasets: [{
                            label: "Replies",
                            data: aDatasets1,
                            backgroundColor: 'blue'
                        }]
                    },
                    options: {
                        scaleShowValues: true,
                        scales: {
                            y: { ticks: { beginAtZero: true } },
                            x: { ticks: { autoSkip: false } }
                        }
                    }
                });
            }
        });
    }
    , GetPostsByUser() {
        $.ajax({
            type: "POST",
            url: SnitzVars.baseUrl + "/Charts/PostsByUser",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (chData) {
                document.getElementById('spinner_posts').style.display = 'none';
                document.getElementById('chart_posts').style.display = 'block';
                var aData = chData;
                var aLabels = aData[0];
                var aDatasets1 = aData[1];

                var ctx = $("#chart_posts");
                var postsChart = new Chart(ctx, {
                    type: 'bar',
                    autoSkip: false,
                    animation: false,
                    showDataPoints: true,
                    responsive: true,
                    data: {
                        labels: aLabels,
                        datasets: [{
                            label: "Posts",
                            data: aDatasets1,
                            backgroundColor: 'blue'
                        }]
                    },
                    options: {
                        scaleShowValues: true,
                        scales: {
                            y: { ticks: { beginAtZero: true } },
                            x: { ticks: { autoSkip: false } }
                        }
                    }
                });
            }
        });
    }
    , GetPostsByMonth(year) {
        $.ajax({
            type: "POST",
            url: SnitzVars.baseUrl + "/Charts/PostsByMonth/" + year,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: function (data) {
                console.log(data);
            },
            success: function (chData) {
                document.getElementById('spinner_months').style.display = 'none';
                document.getElementById('chart_months').style.display = 'block';
                var aData = chData;
                var aLabels = [];
                $.each(aData[0], function (index, value) {
                    aLabels.push(MONTHS[parseInt(value) - 1]);
                });
                var aDatasets1 = aData[1];
                let chartStatus = Chart.getChart("chart_months"); // <canvas> id
                if (chartStatus != undefined) {
                    chartStatus.destroy();
                }
                var ctx = $("#chart_months");
                var monthChart = new Chart(ctx, {
                    type: 'bar',
                    autoSkip: false,
                    animation: false,
                    showDataPoints: true,
                    responsive: true,
                    data: {
                        labels: aLabels,
                        datasets: [{
                            label: "Posts this Month",
                            data: aDatasets1,
                            backgroundColor: '#191970'
                        }]
                    },
                    options: {
                        scaleShowValues: true,
                        scales: {
                            y: { ticks: { beginAtZero: true } },
                            x: { ticks: { autoSkip: false } }
                        }
                    }
                });
            }
        });
    }
    , GetPostsByYear() {
        $.ajax({
            type: "POST",
            url: SnitzVars.baseUrl + "/Charts/PostsByYear",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (chData) {
                // Hide spinner Show chart
                document.getElementById('spinner_years').style.display = 'none';
                document.getElementById('chart_years').style.display = 'block';
                var aData = chData;
                var aLabels = aData[0];
                var aDatasets1 = aData[1];
                var canvas = document.getElementById("chart_years");

                var ctx = $("#chart_years");
                var yearChart = new Chart(ctx, {
                    type: 'bar',
                    autoSkip: false,
                    animation: false,
                    showDataPoints: true,
                    responsive: true,
                    data: {
                        labels: aLabels,
                        datasets: [{
                            label: "Posts this Year",
                            data: aDatasets1,
                            backgroundColor: 'blue'
                        }]
                    },
                    options: {
                        scaleShowValues: true,
                        scales: {
                            y: { ticks: { beginAtZero: true } },
                            x: { ticks: { autoSkip: false } }
                        }
                    }
                });
                canvas.onclick = function (evt) {
                    var activePoints = yearChart.getElementsAtEventForMode(evt, 'nearest', { intersect: true }, false);
                    if (activePoints.length > 0) {
                        var firstPoint = activePoints[0];
                        var label = yearChart.data.labels[firstPoint.index];
                        if ($("#year-select")) {
                            $("#year-select").val(label);
                        }
                        document.getElementById('spinner_months').style.display = 'block';
                        document.getElementById('chart_months').style.display = 'none';
                        SnitzCharts.GetPostsByMonth(label);
                    }
                };
            }
        });
    }
}
