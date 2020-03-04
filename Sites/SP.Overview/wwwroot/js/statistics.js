"use strict";
var baseUrl = "http://local.api.sp.bakx.ca";

window.chartColors = [];

window.onload = function () {

    // Create colors
    window.chartColors.push({ name: "Brilliant Azure", color: "rgb(55, 138, 255)" });
    window.chartColors.push({ name: "Deep Saffron", color: "rgb(255, 163, 47)" });
    window.chartColors.push({ name: "Tart Orange", color: "rgb(245, 79, 82)" });
    window.chartColors.push({ name: "Kiwi", color: "rgb(147, 240, 59)" });
    window.chartColors.push({ name: "Lavender Indigo", color: "rgb(149, 82, 234)" });
    window.chartColors.push({ name: "Chinese Orange", color: "rgb(246, 109, 68)" });
    window.chartColors.push({ name: "Rajah", color: "rgb(254, 174, 101)" });
    window.chartColors.push({ name: "Key Lime", color: "rgb(230, 246, 157)" });
    window.chartColors.push({ name: "Light Moss Green", color: "rgb(170, 222, 167)" });
    window.chartColors.push({ name: "Green Sheen", color: "rgb(100, 194, 166)" });
    window.chartColors.push({ name: "Cyan Cornflower Blue", color: "rgb(45, 135, 187)" });
    window.chartColors.push({ name: "Banana Yellow", color: "rgb(255, 236, 33)" });
    window.chartColors.push({ name: "Princeton Orange", color: "rgb(244, 122, 31)" });
    window.chartColors.push({ name: "Saffron", color: "rgb(253, 187, 47)" });
    window.chartColors.push({ name: "Japanese Laurel", color: "rgb(55, 123, 43)" });
    window.chartColors.push({ name: "Apple", color: "rgb(122, 193, 66)" });
    window.chartColors.push({ name: "Ocean Boat Blue", color: "rgb(0, 124, 195)" });
    window.chartColors.push({ name: "USAFA Blue", color: "rgb(0, 82, 155)" });

    // Load statistics
    loadStatistics();
};


function loadStatistics() {
    getData("countriesChart", "Top 10 countries", baseUrl + "/statistics/GetTopCountries", "country", createDoughnutChart);
    getData("citiesChart", "Top 10 cities", baseUrl + "/statistics/GetTopCities", "city", createDoughnutChart);
    getData("ipChart", "Top 10 ip", baseUrl + "/statistics/GetTopIps", "ipAddress", createDoughnutChart);

    getData("attemptsPerHourChart", "", baseUrl + "/statistics/GetAttemptsPerHour", "key", createLineChart);
    getData("attemptsPerDayChart", "", baseUrl + "/statistics/GetAttemptsPerDay", "key", createLineChart);

    getData("blocksPerHourChart", "", baseUrl + "/statistics/GetBlocksPerHour", "key", createLineChart);
    getData("blocksPerDayChart", "", baseUrl + "/statistics/GetBlocksPerDay", "key", createLineChart);

}

function createDoughnutChart(elem, title, data, dataElement) {

    var config = {
        type: "doughnut",
        data: {
            datasets: [],
            labels: []
        },
        options: {
            responsive: true,
            legend: {
                position: "bottom"
            },
            title: {
                display: true,
                text: title
            },
            animation: {
                animateScale: true,
                animateRotate: true
            }
        }
    };

    var ctx = document.getElementById(elem).getContext("2d");
    var chart = new Chart(ctx, config);

    // Create empty data set
    var dataset = {
        data: [],
        backgroundColor: []
    };

    for (let i = 0; i < data.length; i++) {

        // Populate data
        dataset.data.push(data[i].attempts);

        // Add color
        dataset.backgroundColor.push(getColor(i).color);

        // Add label
        config.data.labels.push(data[i][dataElement]);
    }

    // Add dataset
    config.data.datasets.push(dataset);

    // Update pie chart
    chart.update();
}

function createLineChart(elem, title, data, dataElement) {

    var config = {
        type: "line",
        data: {
            datasets: [],
            labels: []
        },
        options: {
            responsive: true,
            legend: false,
            title: {
                display: true,
                text: title
            },
            animation: {
                animateScale: true,
                animateRotate: true
            }
        }
    };

    var ctx = document.getElementById(elem).getContext("2d");
    var chart = new Chart(ctx, config);

    // Create empty data set
    var dataset = {
        data: [],
        backgroundColor: [],
        fill: false,
    };

    for (let i = 0; i < data.length; i++) {

        // Populate data
        dataset.data.push(data[i].attempts);

        // Add color
        dataset.backgroundColor.push(getColor(0).color);

        // Add label
        config.data.labels.push(data[i][dataElement]);
    }

    // Add data set
    config.data.datasets.push(dataset);

    // Update pie chart
    chart.update();
}

function getData(elem, title, url, dataElement, callback) {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", url);
    xhr.onload = function () {
        if (xhr.status === 200) {
            callback(elem, title, JSON.parse(xhr.responseText), dataElement);
            return;
        }

        alert("Request failed.  Returned status of " + xhr.status);
    };
    xhr.send();
}

function getColor(i) {
    if (i >= window.chartColors.length) {
        console.warn("getColor() out of bounds");
        i = 0;
    }

    return window.chartColors[i];
}