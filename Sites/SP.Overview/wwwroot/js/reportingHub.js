﻿"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/reportingHub").build();
var maxLength = 25;

var hasLoginAttempt = false;
var hasBlock = false;
var hasUnblock = false;

connection.on("ReportLoginAttempt",
    function (attemptId, attemptIpAddress, attemptEventDate, attemptDetails) {

        if (!hasLoginAttempt) {
            document.getElementById("loginAttempts").innerHTML = "";
            hasLoginAttempt = true;
        }

        // Get reference to login attempts
        var elem = document.getElementById("loginAttempts");

        var card = document.createElement("div");
        card.className = "ui card";
        card.alt = attemptId;

        // Create title
        var title = document.createElement("div");
        title.className = "content";

        var titleHeader = document.createElement("div");
        titleHeader.className = "header";
        titleHeader.appendChild(document.createTextNode(attemptIpAddress));
        title.appendChild(titleHeader);

        card.appendChild(title);

        // Create body
        var cardBody = document.createElement("div");
        cardBody.className = "content";

        // Add subtitle
        var subtitle = document.createElement("h4");
        subtitle.className = "ui sub header";
        subtitle.appendChild(document.createTextNode(attemptEventDate));

        cardBody.appendChild(subtitle);

        // Add feed
        var cardFeed = document.createElement("div");
        cardFeed.className = "ui small feed";

        // Details
        cardFeed.appendChild(createEventCard(attemptDetails));

        // Add feed to body
        cardBody.appendChild(cardFeed);

        // Add body to card
        card.appendChild(cardBody);

        // Insert card at top
        elem.insertBefore(card, elem.childNodes[0]);

        // Check 
        if (elem.children.length > maxLength) {
            // Remove the last element
            elem.removeChild(elem.childNodes[maxLength]);
        }
    });

function createEventCard(content)
{
    // Add feed
    var cardEvent = document.createElement("div");
    cardEvent.className = "event";

    var cardEventContent = document.createElement("div");
    cardEventContent.className = "content";

    var cardEventSummary = document.createElement("div");
    cardEventSummary.className = "summary";
    cardEventSummary.appendChild(document.createTextNode(content));

    cardEventContent.appendChild(cardEventSummary);
    cardEvent.appendChild(cardEventContent);

    return cardEvent;
}

connection.on("ReportBlock",
    function (blockId, blockDate, blockDetails, blockIpAddress, blockCity, blockCountry, blockISP) {

        if (!hasBlock) {
            document.getElementById("blockLoader").innerHTML = "";
            hasBlock = true;
        }

        // Get reference to login attempts
        var elem = document.getElementById("blocks");

        var card = document.createElement("div");
        card.className = "ui card";
        card.alt = blockId;

        // Create title
        var title = document.createElement("div");
        title.className = "content";

        var titleHeader = document.createElement("div");
        titleHeader.className = "header";
        titleHeader.appendChild(document.createTextNode(blockCountry + ", " + blockCity));
        title.appendChild(titleHeader);

        card.appendChild(title);

        // Create body
        var cardBody = document.createElement("div");
        cardBody.className = "content";

        // Add subtitle
        var subtitle = document.createElement("h4");
        subtitle.className = "ui sub header";
        subtitle.appendChild(document.createTextNode(blockDate));

        cardBody.appendChild(subtitle);

        // Add feed
        var cardFeed = document.createElement("div");
        cardFeed.className = "ui small feed";

        // IP
        cardFeed.appendChild(createEventCard(blockIpAddress));

        // Details
        cardFeed.appendChild(createEventCard(blockDetails));

        // ISP
        cardFeed.appendChild(createEventCard(blockISP));

        // Add feed to body
        cardBody.appendChild(cardFeed);

        // Add body to card
        card.appendChild(cardBody);

        // Insert card at top
        elem.insertBefore(card, elem.childNodes[0]);

        // Check 
        if (elem.children.length > maxLength) {
            // Remove the last element
            elem.removeChild(elem.childNodes[maxLength]);
        }
    });

connection.on("ReportUnblock",
    function (blockId, blockDate, blockDetails, blockIpAddress, blockCity, blockCountry, blockISP) {

        if (!hasUnblock) {
            document.getElementById("unblockLoader").innerHTML = "";
            hasUnblock = true;
        }

        // Get reference to login attempts
        var elem = document.getElementById("unblocks");

        var card = document.createElement("div");
        card.className = "ui card";
        card.alt = blockId;

        // Create title
        var title = document.createElement("div");
        title.className = "content";

        var titleHeader = document.createElement("div");
        titleHeader.className = "header";
        titleHeader.appendChild(document.createTextNode(blockCountry + ", " + blockCity));
        title.appendChild(titleHeader);

        card.appendChild(title);

        // Create body
        var cardBody = document.createElement("div");
        cardBody.className = "content";

        // Add subtitle
        var subtitle = document.createElement("h4");
        subtitle.className = "ui sub header";
        subtitle.appendChild(document.createTextNode(blockDate));

        cardBody.appendChild(subtitle);

        // Add feed
        var cardFeed = document.createElement("div");
        cardFeed.className = "ui small feed";

        // IP
        cardFeed.appendChild(createEventCard(blockIpAddress));

        // Details
        cardFeed.appendChild(createEventCard(blockDetails));

        // ISP
        cardFeed.appendChild(createEventCard(blockISP));

        // Add feed to body
        cardBody.appendChild(cardFeed);

        // Add body to card
        card.appendChild(cardBody);

        // Insert card at top
        elem.insertBefore(card, elem.childNodes[0]);

        // Check 
        if (elem.children.length > maxLength) {
            // Remove the last element
            elem.removeChild(elem.childNodes[maxLength]);
        }
    });

connection.start().then(function () {
    console.log("Connected to ReportingHub");
}).catch(function (err) {
    return console.error(err.toString());
});