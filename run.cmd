@echo off
set tasks=build test package install uninstall forceinstall
for %%a in (%tasks%) do (
    if %1==%%a goto %1
)

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
    dotnet publish -c release -o .\deploy\bin\netcoreapp3.1 src\Gauge.Dotnet.csproj -f netcoreapp3.1
    dotnet publish -c release -o .\deploy\bin\net6.0 src\Gauge.Dotnet.csproj -f net6.0
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
