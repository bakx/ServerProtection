# What is Service Protect?
Service Protect monitors various system resources to detect brute force login attempts. 

## Plugins
It's using a plugin architecture and comes with the following plugins.

### Detection - EventMonitor
Windows Only: Connects to the Event Log of Windows server (requires Administrative permissions) and fires an LoginAttempt event when it detects
that a login failure occured of type 4625.  

### Reporting - AbuseIP
Reports the hacking attempt to www.abuseipdb.com

## Developer notes
This project is using SQLite in combination with EF Core. More details at : https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli