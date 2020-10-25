"use strict";
var baseUrl = "http://api.overview.serverprotection.local";

window.chartColors = [];

window.onload = function() {

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
    getData("attacksChart", "", baseUrl + "/statistics/GetTopAttacks", "attackType", createDoughnutChart, getAttackType);
    getData("countriesChart", "", baseUrl + "/statistics/GetTopCountries", "country", createDoughnutChart, null);
    getData("citiesChart", "", baseUrl + "/statistics/GetTopCities", "city", createDoughnutChart, null);
    getData("ipChart", "", baseUrl + "/statistics/GetTopIps", "ipAddress", createDoughnutChart, null);

    getData("attackTypesAttemptsPerHourChart", "", baseUrl + "/statistics/GetAttackTypesAttemptsPerHour", "key", createMultiLineChart, null);
    getData("attemptsPerDayChart", "", baseUrl + "/statistics/GetAttemptsPerDay", "key", createLineChart, null);

    getData("blocksPerHourChart", "", baseUrl + "/statistics/GetBlocksPerHour", "key", createLineChart, null);
    getData("blocksPerDayChart", "", baseUrl + "/statistics/GetBlocksPerDay", "key", createLineChart, null);

    getData("last10AccessAttemptsList", "", baseUrl + "/statistics/GetLatestAttempts", "eventDate", populateList, null);
    getData("last10BlockList", "", baseUrl + "/statistics/GetLatestBlocks", "eventDate", populateList, null);
}

function createDoughnutChart(elem, title, data, dataElement, dataElementMapping) {

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
    var dataSet = {
        data: [],
        backgroundColor: []
    };

    for (let i = 0; i < data.length; i++) {

        // Populate data
        dataSet.data.push(data[i].attempts);

        // Add color
        dataSet.backgroundColor.push(getColor(i).color);

        // Add label

        // Check if conversion is needed
        if (dataElementMapping !== null) {
            config.data.labels.push(dataElementMapping(data[i][dataElement]));
        } else {
            config.data.labels.push(data[i][dataElement]);
        }
    }

    // Add dataSet
    config.data.datasets.push(dataSet);

    // Update pie chart
    chart.update();
}

function createMultiLineChart(elem, title, data, dataElement) {

    var config = {
        type: "line",
        data: {
            datasets: [],
            labels: []
        },
        options: {
            responsive: true,

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
    var dataSet = {};

    for (let i = 0; i < data.length; i++) {

        // Create empty data set
        dataSet = {
            data: [],
            backgroundColor: [],
            fill: false,
        };

           // Add label
        dataSet.label = data[i][dataElement];

        for (let j = 0; j < data[i].data.length; j++) {

            if (i === 0) {
                config.data.labels.push(data[i].data[j].key);
                }


            // Populate data
            dataSet.data.push(data[i].data[j].attempts);

            // Add color
            dataSet.borderColor = getColor(i).color;
            dataSet.backgroundColor = getColor(i).color;
        }

        // Add data set
        config.data.datasets.push(dataSet);
    }

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
    var dataSet = {
        data: [],
        backgroundColor: [],
        fill: false,
    };

    for (let i = 0; i < data.length; i++) {

        // Populate data
        dataSet.data.push(data[i].attempts);

        // Add color
        dataSet.backgroundColor.push(getColor(0).color);

        // Add label
        config.data.labels.push(data[i][dataElement]);
    }

    // Add data set
    config.data.datasets.push(dataSet);

    // Update pie chart
    chart.update();
}

function populateList(elem, title, data, dateElement) {

    var target = document.getElementById(elem);

    for (let i = 0; i < data.length; i++) {
        target.appendChild(createListItem(getAttackType(data[i].attackType), data[i].details, data[i].ipAddress, new Date(data[i][dateElement]).toLocaleString()));
    }
}

function createListItem(type, details, ipAddress, date) {

    // Create card
    var card = document.createElement("div");
    card.className = "item";

    // Content div
    var cardContent = document.createElement("div");
    cardContent.className = "content";

    var cardHeader = document.createElement("div");
    cardHeader.className = "header";
    cardHeader.appendChild(document.createTextNode(date));

    cardContent.appendChild(cardHeader);
    cardContent.appendChild(document.createTextNode(details));

    var cardIpAddress = document.createElement("div");
    cardIpAddress.appendChild(document.createTextNode(ipAddress));

    var cardType = document.createElement("div");
    cardType.appendChild(document.createTextNode(type));
    
    cardContent.appendChild(cardIpAddress);
    cardContent.appendChild(cardType);

    card.appendChild(cardContent);
    return card;
}

function getData(elem, title, url, dataElement, callback, dataElementMapping) {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", url);
    xhr.onload = function() {
        if (xhr.status === 200) {
            callback(elem, title, JSON.parse(xhr.responseText), dataElement, dataElementMapping);
            return;
        }

        alert("Request failed.  Returned status of " + xhr.status);
    };
    xhr.send();
}

function getAttackType(i) {
    switch (i) {
        case 1:
            return "Web Spam";
        case 2:
            return "Port Scan";
        case 3:
            return "Sql Injection";
        case 4:
            return "Brute-Force";
        case 5:
            return "Web Exploit";
        default:
            return "Not specified";
    }
}

function getColor(i) {
    if (i >= window.chartColors.length) {
        console.warn("getColor() out of bounds");
        i = 0;
    }

    return window.chartColors[i];
}