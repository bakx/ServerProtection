# What is Service Protect?
Service Protect monitors various system resources to detect brute force login attempts. 

## Plugins
It's using a plugin architecture and comes with the following plugins.

### Detection - EventMonitor
Windows Only: Connects to the Event Log of Windows server (requires Administrative permissions) and fires an LoginAttempt event when it detects
that a login failure occured of type 4625.  

### Reporting - AbuseIP
Reports the hacking attempt to www.abuseipdb.com



## Ignore below
sc create TestService BinPath=C:\full\path\to\publish\dir\WindowsServiceExample.exe

The SC command is a bog standard windows command (Has nothing to do with .NET Core), that installs a windows service. We say that we are creating a windows service, and we want to call it “TestService”. Importantly, we pass in the full path to our windows service exe.

We should be greeted with :

[SC] CreateService SUCCESS

Then all we need to do is start our service :

sc start TestService

We can now check our log file and see our service running!

To stop and delete our service, all we need to do is :

sc stop TestService
sc delete TestService