@echo off
set tasks=build, test, package, install, uninstall, forceinstall

if [%1] == [] goto :usage

for %%a in (%tasks%) do (
	if %%a==%1 goto %1
)

:usage
echo Options: "[build | test | package | install | uninstall | forceinstall]"
goto :eof

:build
    dotnet build -c release
    goto :eof

:test
    dotnet test --no-build -c release test\Gauge.Dotnet.UnitTests.csproj
    if %ERRORLEVEL% GEQ 1 exit %ERRORLEVEL%
    dotnet test --no-build -c release integration-test\Gauge.Dotnet.IntegrationTests.csproj
    if %ERRORLEVEL% GEQ 1 exit %ERRORLEVEL%
    goto :eof

:package
    rmdir /s /q deploy artifacts
    dotnet publish -c release -o .\deploy\bin\net8.0 src\Gauge.Dotnet.csproj -f net8.0
    dotnet publish -c release -o .\deploy\bin\net9.0 src\Gauge.Dotnet.csproj -f net9.0
    copy src\launcher.sh deploy\
    copy src\launcher.cmd deploy\
    copy src\dotnet.json deploy\
    mkdir artifacts
    call :powershell zip
    goto :eof

:install
    call :package
    call :powershell install
    goto :eof

:forceinstall
    call :uninstall
    call :install
    goto :eof

:uninstall
    call :powershell uninstall
    goto :eof

:powershell
    powershell.exe -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile -Command "& '.\%~1.ps1'"
    if %errorlevel% neq 0 exit /b %errorlevel%
