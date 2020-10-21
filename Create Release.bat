@echo off
set releasePath="E:\Releases\SP.Protect"
set releaseArchive="E:\Releases"
set configurationName="Release"

echo ### Removing %releasePath%
rmdir /s /q %releasePath%

echo ### Creating %releasePath%
mkdir /s /q %releasePath%

echo ### Removing bin folders
rmdir /s /q "SP.Core/bin"

echo ### Building solution
dotnet build SP.Core\SP.Core\SP.Core.csproj /t:Restore,Clean,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
dotnet build SP.Core\Plugins\Api\Api.Https\Plugins.Api.Https.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
dotnet build SP.Core\Plugins\Api\Api.gRPC\Plugins.Api.gRPC.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
dotnet build SP.Core\Plugins\Detection\Windows.Event.Monitor\Plugins.Windows.Event.Monitor.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
dotnet build SP.Core\Plugins\Detection\Windows.IIS.Monitor\Plugins.Windows.IIS.Monitor.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
dotnet build SP.Core\Plugins\Reporting\AbuseIP\Plugins.AbuseIP.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
dotnet build SP.Core\Plugins\Reporting\LiveReport.SignalR\Plugins.LiveReport.SignalR.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
msbuild.exe  SP.Core\Plugins\System\Windows.Firewall\Plugins.Windows.Firewall.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU

echo ###
echo ### Copying files from Configuration project to release folder
echo ###

xcopy "SP.Core\SP.Core\bin\%configurationName%\netcoreapp3.1" "%releasePath%\" /E
xcopy "SP.Core\Plugins\Api\Api.Https\bin\%configurationName%\netcoreapp3.1" "%releasePath%\plugins\api.https\" /E
xcopy "SP.Core\Plugins\Api\Api.gRPC\bin\%configurationName%\netcoreapp3.1" "%releasePath%\plugins\api.gRPC\" /E
xcopy "SP.Core\Plugins\Detection\Windows.Event.Monitor\bin\%configurationName%\netcoreapp3.1" "%releasePath%\plugins\windows.event.monitor\" /E
xcopy "SP.Core\Plugins\Detection\Windows.IIS.Monitor\bin\%configurationName%\netcoreapp3.1" "%releasePath%\plugins\windows.iis.monitor\" /E
xcopy "SP.Core\Plugins\Reporting\AbuseIP\bin\%configurationName%\netcoreapp3.1" "%releasePath%\plugins\abuseip\" /E
xcopy "SP.Core\Plugins\Reporting\LiveReport.SignalR\bin\%configurationName%\netcoreapp3.1" "%releasePath%\plugins\livereport.signalr\" /E
xcopy "SP.Core\Plugins\System\Windows.Firewall\bin\%configurationName%\netcoreapp3.1" "%releasePath%\plugins\windows.firewall\" /E

echo ###
echo ### Singing dlls
echo ###

signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Core.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Core.exe"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\api.https\Plugins.Api.Https.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\api.grpc\Plugins.Api.gRPC.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\windows.eventmonitor\Plugins.Windows.Event.Monitor.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\windows.eventmonitor\Plugins.Windows.IIS.Monitor.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\windows.firewall\Plugins.Windows.Firewall.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\abuseip\Plugins.AbuseIP.dll"

echo ###
echo ### Cleaning up configurations - SP.Core
echo ###

del "%releasePath%\Config\appSettings.development.hjson" /s /f /q
del "%releasePath%\Config\appSettings.development.json" /s /f /q
ren "%releasePath%\Config\appSettings.hjson" "sample.appSettings.hjson"
ren "%releasePath%\Config\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\Config\logSettings.json" "sample.logSettings.json"

echo ### Cleaning up configurations - Plugins
del "%releasePath%\plugins\abuseip\appSettings.development.json" /s /f /q
ren "%releasePath%\plugins\abuseip\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\abuseip\logSettings.json" "sample.logSettings.json"

del "%releasePath%\plugins\api.https\appSettings.development.json" /s /f /q
ren "%releasePath%\plugins\api.https\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\api.https\logSettings.json" "sample.logSettings.json"

del "%releasePath%\plugins\api.grpc\appSettings.development.json" /s /f /q
ren "%releasePath%\plugins\api.grpc\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\api.grpc\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\plugins\windows.event.monitor\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\windows.event.monitor\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\plugins\windows.iis.monitor\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\windows.iis.monitor\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\plugins\windows.firewall\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\windows.firewall\logSettings.json" "sample.logSettings.json"

del "%releasePath%\plugins\livereport.signalr\appSettings.development.json" /s /f /q
ren "%releasePath%\plugins\livereport.signalr\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\livereport.signalr\logSettings.json" "sample.logSettings.json"

versioninfo "%releasePath%\SP.Core.dll" > version.txt
set /p V=<version.txt

echo ###  Zipping release
7za a -tzip "%releaseArchive%\SP.Core_%V%.zip" "%releasePath%\*"

echo ### Cleanup
del version.txt