@echo off
set tasks=build test package install uninstall forceinstall
for %%a in (%tasks%) do (
    if %1==%%a goto %1
)

echo Options: "[build | test | package | install]"
goto :eof

:build
    dotnet build
    goto :eof

:test
    dotnet build
    dotnet test --no-build Runner.UnitTests\Runner.UnitTests.csproj
    dotnet test --no-build Runner.IntegrationTests\Runner.IntegrationTests.csproj
    goto :eof

:package
    rmdir /s /q deploy artifacts
    dotnet publish -c release -o ..\deploy\bin Runner\Runner.csproj
    cp Runner\dotnet.sh deploy
    cp Runner\dotnet.cmd deploy
    cp Runner\csharp-netstandard.json deploy
    mkdir artifacts
    call :powershell zip
    goto :eof

:install
    call :package
    call :powershell install
    goto :eof

:uninstall
    call :powershell uninstall
    goto :eof

:forceinstall
    call :uninstall
    call :install
    goto :eof

:powershell
    powershell.exe -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile -Command "& '.\%~1.ps1'"
    if %errorlevel% neq 0 exit /b %errorlevel%
