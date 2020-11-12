@echo off
set releasePath="E:\Releases\Release"
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
dotnet build SP.Core\Plugins\Detection\Honeypot\Plugins.Honeypot.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
dotnet build SP.Core\Plugins\Reporting\AbuseIP\Plugins.AbuseIP.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
dotnet build SP.Core\Plugins\Reporting\LiveReport.SignalR\Plugins.LiveReport.SignalR.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU
msbuild.exe  SP.Core\Plugins\System\Windows.Firewall\Plugins.Windows.Firewall.csproj /t:Restore,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU

dotnet build Api\SP.Api.Service\SP.Api.Service.csproj /t:Restore,Clean,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU

echo ###
echo ### Copying files from Configuration project to release folder
echo ###

xcopy "SP.Core\SP.Core\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\" /E
xcopy "SP.Core\Plugins\Api\Api.Https\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\plugins\api.https\" /E
xcopy "SP.Core\Plugins\Api\Api.gRPC\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\plugins\api.gRPC\" /E
xcopy "SP.Core\Plugins\Detection\Windows.Event.Monitor\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\plugins\windows.event.monitor\" /E
xcopy "SP.Core\Plugins\Detection\Windows.IIS.Monitor\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\plugins\windows.iis.monitor\" /E
xcopy "SP.Core\Plugins\Detection\Honeypot\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\plugins\honeypot\" /E
xcopy "SP.Core\Plugins\Reporting\AbuseIP\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\plugins\abuseip\" /E
xcopy "SP.Core\Plugins\Reporting\LiveReport.SignalR\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\plugins\livereport.signalr\" /E
xcopy "SP.Core\Plugins\System\Windows.Firewall\bin\%configurationName%\net5.0" "%releasePath%\SP.Protect\plugins\windows.firewall\" /E

xcopy "Api\SP.Api.Service\bin\%configurationName%\net5.0" "%releasePath%\SP.Api.Service\" /E

echo ###
echo ### Singing dlls
echo ###

signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\SP.Core.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\SP.Core.exe"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\plugins\api.https\Plugins.Api.Https.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\plugins\api.grpc\Plugins.Api.gRPC.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\plugins\windows.event.monitor\Plugins.Windows.Event.Monitor.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\plugins\windows.iis.monitor\Plugins.Windows.IIS.Monitor.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\plugins\honeypot\Plugins.Honeypot.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\plugins\abuseip\Plugins.AbuseIP.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Protect\plugins\windows.firewall\Plugins.Windows.Firewall.dll"

signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Api.Service\SP.Api.Service.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Api.Service\SP.Api.Service.exe"

echo ###
echo ### Cleaning up configurations - SP.Core
echo ###

del "%releasePath%\SP.Protect\config\appSettings.development.hjson" /s /f /q
del "%releasePath%\SP.Protect\config\appSettings.development.json" /s /f /q
ren "%releasePath%\SP.Protect\config\appSettings.hjson" "sample.appSettings.hjson"
ren "%releasePath%\SP.Protect\config\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\config\logSettings.json" "sample.logSettings.json"

echo ### Cleaning up configurations - Plugins
del "%releasePath%\SP.Protect\plugins\abuseip\appSettings.development.json" /s /f /q
ren "%releasePath%\SP.Protect\plugins\abuseip\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\plugins\abuseip\logSettings.json" "sample.logSettings.json"

del "%releasePath%\SP.Protect\plugins\api.https\appSettings.development.json" /s /f /q
ren "%releasePath%\SP.Protect\plugins\api.https\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\plugins\api.https\logSettings.json" "sample.logSettings.json"

del "%releasePath%\SP.Protect\plugins\api.grpc\appSettings.development.json" /s /f /q
ren "%releasePath%\SP.Protect\plugins\api.grpc\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\plugins\api.grpc\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\SP.Protect\plugins\windows.event.monitor\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\plugins\windows.event.monitor\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\SP.Protect\plugins\windows.iis.monitor\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\plugins\windows.iis.monitor\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\SP.Protect\plugins\honeypot\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\plugins\honeypot\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\SP.Protect\plugins\windows.firewall\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\plugins\windows.firewall\logSettings.json" "sample.logSettings.json"

del "%releasePath%\SP.Protect\plugins\livereport.signalr\appSettings.development.json" /s /f /q
ren "%releasePath%\SP.Protect\plugins\livereport.signalr\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Protect\plugins\livereport.signalr\logSettings.json" "sample.logSettings.json"

del "%releasePath%\SP.Api.Service\config\appSettings.development.json" /s /f /q
ren "%releasePath%\SP.Api.Service\config\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\SP.Api.Service\config\logSettings.json" "sample.logSettings.json"

versioninfo "%releasePath%\SP.Protect\SP.Core.dll" > version.txt
set /p V=<version.txt

echo ###  Zipping release
7za a -tzip "%releaseArchive%\SP.Core_%V%.zip" "%releasePath%\*"

echo ### Cleanup
del version.txt