@echo off

FOR /F "delims=" %%i IN ('dotnet --version') DO set DOTNET_VER=%%i
if "3." == "%DOTNET_VER:~0,2%" goto :net3

dotnet bin\net5.0\Gauge.Dotnet.dll %*
if %errorlevel% neq 0 exit /b %errorlevel%
goto :eof

:net3
    dotnet bin\netcoreapp3.0\Gauge.Dotnet.dll %*
    if %errorlevel% neq 0 exit /b %errorlevel%
