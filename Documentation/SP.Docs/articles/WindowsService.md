# Add your introductions here!

Service Installation

@echo off
sc create "Server Protection Core" start="Auto" BinPath="%cd%\SP.Core.exe"
sc description "Server Protection Core" "Service Protection monitors various system resources to detect brute force login attempts. Integrates with the Windows Firewall to automatically block unwanted connections."
sc start "Server Protection Core"


@echo off
sc stop "Server Protection Core"
sc delete "Server Protection Core"

@echo off
sc start "Server Protection Core"

@echo off
sc stop "Server Protection Core"
