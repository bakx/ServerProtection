# What is Service Protect?
Service Protect monitors various system resources to detect brute force login attempts. 

## Sites
It's using a plugin architecture and comes with the following plugins.

### SP.API
The API project contains the database that's used to store all data related to the project. It's essential for the Core project to operate.

### SP.Overview
Provides an overview of the login attempts made on this server. It includes live data (if the LiveReport.SignalR plug-in is enabled) and various statistics 
related to the login attempts.

## Plugins
It's using a plugin architecture and comes with the following plugins.

### Detection - EventMonitor
Windows Only: Connects to the Event Log of Windows server (requires Administrative permissions) and fires an LoginAttempt event when it detects
that a login failure occured of type 4625.  

### Reporting - AbuseIP
Reports the hacking attempt to www.abuseipdb.com

### Reporting - LiveReport.SignalR
Reports the hacking attempt to the SignalR site that's part of this solution

## Developer notes
This project is using SQLite in combination with EF Core. More details at : https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

## Acknowledgements
Shield icon taken from http://www.iconarchive.com/show/small-n-flat-icons-by-paomedia/shield-icon.html