@echo off
REM Ask for Release version unless supplied by first commandline arg
if "%1" == "" (
	set /p version="Version: "
) else (
	set version=%1
)
msbuild S7.Net\S7.Net.csproj /P:Configuration=Release
rmdir /S /Q nuget-pack\lib\%version%
xcopy S7.Net\bin\Release\net452\S7.Net.dll nuget-pack\lib\%version%\net452\ /Y 
xcopy S7.Net\bin\Release\net452\S7.Net.xml nuget-pack\lib\%version%\net452\ /Y 
xcopy S7.Net\bin\Release\netstandard2.0\S7.Net.dll nuget-pack\lib\%version%\netstandard2.0\ /Y 
xcopy S7.Net\bin\Release\netstandard2.0\S7.Net.xml nuget-pack\lib\%version%\netstandard2.0\ /Y 
.nuget\nuget pack nuget-pack\S7.Net.nuspec -Version %version%
move S7netplus.%version%.nupkg nuget-pack\lib\%version%\
pause