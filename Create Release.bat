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
msbuild.exe SP.Core/SP.Core.csproj /t:Clean,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/Api/ApiHttps/Plugins.Api.Https.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/Api/ApiGRPC/Plugins.Api.gRPC.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/Detection/WindowsEventMonitor/Plugins.Windows.EventMonitor.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/System/WindowsFirewall/Plugins.Windows.Firewall.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/Reporting/AbuseIP/Plugins.AbuseIP.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/Reporting/LiveReport.SignalR/Plugins.LiveReport.SignalR.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"


echo ### Copying files from Configuration project to release folder
xcopy "SP.Core\bin\%configurationName%\netcoreapp3.1" "%releasePath%\" /E

echo ### Singing dlls
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Core.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\SP.Core.exe"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\api.https\Plugins.Api.Https.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\api.grpc\Plugins.Api.gRPC.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\windows.eventmonitor\Plugins.Windows.EventMonitor.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\windows.firewall\Plugins.Windows.Firewall.dll"
signtool.exe sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\abuseip\Plugins.AbuseIP.dll"

echo ### Cleaning up configurations - SP.Core
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

ren "%releasePath%\plugins\windows.eventmonitor\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\windows.eventmonitor\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\plugins\windows.firewall\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\windows.firewall\logSettings.json" "sample.logSettings.json"

del "%releasePath%\plugins\liveReport.signalR\appSettings.development.json" /s /f /q
ren "%releasePath%\plugins\liveReport.signalR\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\liveReport.signalR\logSettings.json" "sample.logSettings.json"

versioninfo "%releasePath%\SP.Core.dll" > version.txt
set /p V=<version.txt

echo ###  Zipping release
7za a -tzip "%releaseArchive%\SP.Core_%V%.zip" "%releasePath%\*"

echo ### Cleanup
del version.txt