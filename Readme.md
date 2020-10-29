# **Server Protection**
Server Protection monitors various system resources to detect brute force login attempts. The purpose of this project is to block such attempts to prevent unauthorized access.

It's using a plug-in architecture and can be extended to support various 3rd party tools.

The current builds are focusing on Windows (Server) where ideally it should get installed to run as a Windows service.

## **Api**
To support a wide range of machines the *api* project is split up between 2 projects. Only one of these has to be installed.
They both come with a matching plug-in.

### **SP.Api.Https**
The **SP.Api.Https** project uses the REST protocol and can be hosted through IIS or other webservers that can host .NET Core.
It contains all the *api* calls required for the project to work and is responsible for storing and exposing the data.

### **SP.Api.gRPC**
The **SP.Api.gRPC** project uses the gRPC protocol and can be hosted through IIS or other webservers that can host .NET Core, but also as a *Windows Service*.
It contains all the *api* calls required for the project to work and is responsible for storing and exposing the data.

It's recommended to use the **SP.Api.gRPC** instead of the **SP.Api.Https**.

## **SP.Core**
This project contains various individual components which are set up to work together. They are divided between SP.Core, Api's and Plug-ins.

The base of Server Protection is the SP.Core project.

### **SP.Core**
The Core is considered the base program and loads all enabled plug-ins. It should ideally be run as a service. 

### **SP.Models**
Contains all data models used by the project.

## **Plugins**
Server Protection comes with the following plugins:

### **Api**

#### **Plugins.Api.Https**
Communicates with the **SP.Api.Https** when enabled.

#### **Plugins.Api.gRPC**
Communicates with the **SP.Api.gRPC** when enabled.

### **Detection**

#### **Plugins.Windows.Event.Monitor**
**Windows Only:** Connects to the Event Log of Windows server (requires Administrative permissions) and fires an AccessAttempt event when it detects
that a login failure occured of type 4625.

#### **Plugins.Windows.IIS.Monitor**
**Windows Only:** Connects to the Event Log of Windows server (requires Administrative permissions) and fires an AccessAttempt and BlockEvent event when it detects
that a visitor requests specific paths through the web server.

#### **Plugins.Honeypot**
Sets up a TCP listener on configured ports and fires an AccessAttempt and BlockEvent event when it detects that a visitor attempted a connection on that port.

### **Reporting**
#### **Plugins.AbuseIP**
Reports the hacking attempt to www.abuseipdb.com

#### **Plugins.LiveReport.SignalR**
Reports the hacking attempt to the SP.Overview site that's part of this solution

### **System**
#### **Plugins.Windows.Firewall**
**Windows Only:** Connects to the Windows Firewall and handles IP blocks/unblocks.

### **Testing**
#### **Plugins.Load.Simulator**
Simplified stress testing that will simulate login failures in a very high rate.

## **Overview**
To provide diagnostics and offer central reporting, this project comes with an overview page that displays various statistics.

### **SP.Overview**
Provides an overview of the login attempts made on this server. It includes live data (if the LiveReport.SignalR plug-in is enabled) and various statistics related to the login attempts.

### **SP.Api.Overview**
Contains the datasource for the SP.Overview project and exposes various statistics related to the login attempts, blocks, ISPs and more.

## **Documentation**

### **SP.Docs**
Uses DocFX to create all documentation of this project.

## Acknowledgements
Shield icon taken from http://www.iconarchive.com/show/small-n-flat-icons-by-paomedia/shield-icon.html

## About
https://www.serverprotection.dev

https://documentation.serverprotection.dev

---

## Future goals - Not implemented
* To support attacks on a complete server park, Server Protection communicates through the SP.API project. This allows system administrators to protect multiple servers simultanously 
when an attack is detected.
* Support Linux and Mac (by adding plug-ins specifically for these platforms)
 * Look into https://otx.alienvault.com/