# What is Server Protect?
Server Protect monitors various system resources to detect brute force login attempts. The purpose of this project is to block such attempts to prevent unauthorized access.

It's using a plugin architecture and can be extended to support various 3rd party tools.

The current builds are focusing on Windows (server) where should be installed to run as a Windows service. 

# **Projects**
All indidivual components of this applications are set up to use their own project. They are divided between Core, Sites, Services and Plug-ins.

# **Main**
The base of the Server Protect is the Core project.

## **SP.Core**
The Core is considered the base program and loads all enabled plug-ins. It should ideally be run as a service. 

## **SP.Models**
Contains all data models used by the project.

# **Sites**
To provide diagnostics and offer central reporting, this project comes with an API and an overview page that displays various statistics.
These are split up in separate projects

## **SP.API**
The API project contains the database that's used to store all data related to the project. 

## **SP.Overview**
Provides an overview of the login attempts made on this server. It includes live data (if the LiveReport.SignalR plug-in is enabled) and various statistics related to the login attempts.

# **Plugins**
Server Protect comes with the following plugins:

### **Detection - EventMonitor**
**Windows Only:** Connects to the Event Log of Windows server (requires Administrative permissions) and fires an LoginAttempt event when it detects
that a login failure occured of type 4625.  

### **Reporting - AbuseIP**
Reports the hacking attempt to www.abuseipdb.com

### **Reporting - LiveReport.SignalR**
Reports the hacking attempt to the SignalR site that's part of this solution

## Acknowledgements
Shield icon taken from http://www.iconarchive.com/show/small-n-flat-icons-by-paomedia/shield-icon.html

---

## Future goals - Not implemented
* To support attacks on a complete server park, Server Protect communicates through the SP.API project. This allows system administrators to protect multiple servers simultanously 
when an attack is detected.
* Support Linux and Mac (by adding plug-ins specifically for these platforms)
