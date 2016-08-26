@echo off
set /p version="Version: "
msbuild S7.Net\S7.Net.csproj /P:Configuration=Release
rmdir /S /Q nuget-pack\lib
xcopy S7.Net\bin\Release\S7.Net.dll nuget-pack\lib\net35\ /Y 
xcopy S7.Net\bin\Release\S7.Net.xml nuget-pack\lib\net35\ /Y 
xcopy S7.Net\bin\Release\S7.Net.dll nuget-pack\lib\net40\ /Y 
xcopy S7.Net\bin\Release\S7.Net.xml nuget-pack\lib\net40\ /Y 
xcopy S7.Net\bin\Release\S7.Net.dll nuget-pack\lib\net45\ /Y 
xcopy S7.Net\bin\Release\S7.Net.xml nuget-pack\lib\net45\ /Y 
.nuget\nuget pack nuget-pack\S7.Net.nuspec -Version %version%
pause