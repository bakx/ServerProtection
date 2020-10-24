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
