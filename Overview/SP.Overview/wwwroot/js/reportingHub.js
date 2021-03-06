﻿"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/reportingHub").build();
var maxLength = 25;

var hasAccessAttempt = false;
var hasBlock = false;
var hasUnblock = false;

connection.on("ReportAccessAttempt",
    function(attemptId, attemptSource, attemptIpAddress, attemptEventDate, attemptDetails, attemptAttackType) {

        // Get reference to login attempts
        var elem = document.getElementById("accessAttempts");

        // Clear loading block
        if (!hasAccessAttempt) {
            elem.innerHTML = "";
            hasAccessAttempt = true;
        }

        // Create card
        var card = document.createElement("div");
        card.className = "ui card fluid";
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
        subtitle.appendChild(document.createTextNode(new Date(attemptEventDate).toLocaleString()));

        cardBody.appendChild(subtitle);

        // Add feed
        var cardFeed = document.createElement("div");
        cardFeed.className = "ui small feed";

        // Source
        cardFeed.appendChild(createEventCard("Source: " + attemptSource));

        // Details
        cardFeed.appendChild(createEventCard("Details: " + attemptDetails));

        // AttackType
        cardFeed.appendChild(createEventCard("Attack Type: " + attemptAttackType));

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

connection.on("ReportBlock",
    function (blockId, blockDate, blockDetails, blockIpAddress, blockCity, blockCountry, blockISP, blockAttackType) {

        // Get reference to blocks
        var elem = document.getElementById("blocks");

        // Clear loading block
        if (!hasBlock) {
            elem.innerHTML = "";
            hasBlock = true;
        }

        // Create card
        var card = document.createElement("div");
        card.className = "ui card fluid";
        card.alt = blockId;

        // Create title
        var title = document.createElement("div");
        title.className = "content";

        var titleHeader = document.createElement("div");
        titleHeader.className = "header";
        titleHeader.appendChild(document.createTextNode(blockIpAddress));
        title.appendChild(titleHeader);

        card.appendChild(title);

        // Create body
        var cardBody = document.createElement("div");
        cardBody.className = "content";

        // Add subtitle
        var subtitle = document.createElement("h4");
        subtitle.className = "ui sub header";
        subtitle.appendChild(document.createTextNode(new Date(blockDate).toLocaleString()));

        cardBody.appendChild(subtitle);

        // Add feed
        var cardFeed = document.createElement("div");
        cardFeed.className = "ui small feed";

        // Location
        cardFeed.appendChild(createEventCard("Location: " + blockCountry + ", " + blockCity));

        // Details
        cardFeed.appendChild(createEventCard("Details: " + blockDetails));

        // ISP
        cardFeed.appendChild(createEventCard("ISP: " + blockISP));

        // Attack Type
        cardFeed.appendChild(createEventCard("Attack Type: " + blockAttackType));

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
    function(blockId, blockDate, blockDetails, blockIpAddress, blockCity, blockCountry, blockISP, blockAttackType) {

        // Get reference to unblocks
        var elem = document.getElementById("unblocks");

        // Clear loading block
        if (!hasUnblock) {
            elem.innerHTML = "";
            hasUnblock = true;
        }

        // Create card
        var card = document.createElement("div");
        card.className = "ui card fluid";
        card.alt = blockId;

        // Create title
        var title = document.createElement("div");
        title.className = "content";

        var titleHeader = document.createElement("div");
        titleHeader.className = "header";
        titleHeader.appendChild(document.createTextNode(blockIpAddress));
        title.appendChild(titleHeader);

        card.appendChild(title);

        // Create body
        var cardBody = document.createElement("div");
        cardBody.className = "content";

        // Add subtitle
        var subtitle = document.createElement("h4");
        subtitle.className = "ui sub header";
        subtitle.appendChild(document.createTextNode(new Date(blockDate).toLocaleString()));

        cardBody.appendChild(subtitle);

        // Add feed
        var cardFeed = document.createElement("div");
        cardFeed.className = "ui small feed";

        // Location
        cardFeed.appendChild(createEventCard("Location: " + blockCountry + ", " + blockCity));

        // Details
        cardFeed.appendChild(createEventCard("Details: " + blockDetails));

        // ISP
        cardFeed.appendChild(createEventCard("ISP: " + blockISP));

        // Attack Type
        cardFeed.appendChild(createEventCard("Attack Type: " + blockAttackType));

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

connection.start().then(function() {
    console.log("Connected to ReportingHub");
}).catch(function(err) {
    return console.error(err.toString());
});


/* */
function createEventCard(content) {
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