@echo off
sc create "Server Protect Core" start="Auto" BinPath="%cd%\SP.Core.exe"
sc description "Server Protect Core" "Service Protect monitors various system resources to detect brute force login attempts. Integrates with the Windows Firewall to automatically block unwanted connections."
sc start "Server Protect Core"
