@echo off
set releasePath="E:\Releases\SP.Protect"
set releaseArchive="E:\Releases"
set configurationName="Release"

echo ### Removing %releasePath%
rmdir /s /q %releasePath%

echo ### Creating %releasePath%
mkdir /s /q %releasePath%

echo ### Removing bin folders
rmdir /s /q "Core/bin"

echo ### Building solution
msbuild.exe Core/Core.csproj /t:Clean,Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/Detection/EventMonitor/Plugins.EventMonitor.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/Reporting/AbuseIP/Plugins.AbuseIP.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"
msbuild.exe Plugins/Detection/SignalR/Plugins.SignalR.csproj /t:Rebuild /p:Configuration=%configurationName% /p:Platform=AnyCPU /p:SolutionDir="%cd%"

echo ### Copying files from Configuration project to release folder
xcopy "Core\bin\%configurationName%\netcoreapp3.1" "%releasePath%\" /E

echo ### Singing dlls
"C:\Program Files (x86)\kSign\signtool.exe" sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\Core.dll"
"C:\Program Files (x86)\kSign\signtool.exe" sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\Core.exe"
"C:\Program Files (x86)\kSign\signtool.exe" sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\eventmonitor\Plugins.EventMonitor.dll"
"C:\Program Files (x86)\kSign\signtool.exe" sign /n "Gideon Bakx" /fd sha256 /t "http://timestamp.comodoca.com" "%releasePath%\plugins\abuseip\Plugins.AbuseIP.dll"

echo ### Cleaning up configurations - Core
del "%releasePath%\Config\appSettings.development.hjson" /s /f /q
del "%releasePath%\Config\appSettings.development.json" /s /f /q
ren "%releasePath%\Config\appSettings.hjson" "sample.appSettings.hjson"
ren "%releasePath%\Config\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\Config\logSettings.json" "sample.logSettings.json"

echo ### Cleaning up configurations - Plugins
del "%releasePath%\plugins\abuseip\appSettings.development.json" /s /f /q
ren "%releasePath%\plugins\abuseip\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\abuseip\logSettings.json" "sample.logSettings.json"

ren "%releasePath%\plugins\eventmonitor\appSettings.json" "sample.appSettings.json"
ren "%releasePath%\plugins\eventmonitor\logSettings.json" "sample.logSettings.json"

versioninfo "%releasePath%\Core.dll" > version.txt
set /p V=<version.txt

echo ###  Zipping release
7za a -tzip "%releaseArchive%\SP.Core_%V%.zip" "%releasePath%\*"

echo ### Cleanup
del version.txt