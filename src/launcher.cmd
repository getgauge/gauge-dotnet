@echo off

FOR /F "delims=" %%i IN ('dotnet --version') DO set DOTNET_VER=%%i
if "6." == "%DOTNET_VER:~0,2%" goto :net6

dotnet bin\net7.0\Gauge.Dotnet.dll %*
if %errorlevel% neq 0 exit /b %errorlevel%
goto :eof

:net6
    dotnet bin\net6.0\Gauge.Dotnet.dll %*
    if %errorlevel% neq 0 exit /b %errorlevel%
