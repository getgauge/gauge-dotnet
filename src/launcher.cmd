@echo off

FOR /F "delims=" %%i IN ('dotnet --version') DO set DOTNET_VER=%%i
if "8." == "%DOTNET_VER:~0,2%" goto :net8
if "9." == "%DOTNET_VER:~0,2%" goto :net9

dotnet bin\net10.0\Gauge.Dotnet.dll %*
if %errorlevel% neq 0 exit /b %errorlevel%
goto :eof

:net8
    dotnet bin\net8.0\Gauge.Dotnet.dll %*
    if %errorlevel% neq 0 exit /b %errorlevel%
    goto :eof

:net9
    dotnet bin\net9.0\Gauge.Dotnet.dll %*
    if %errorlevel% neq 0 exit /b %errorlevel%
    goto :eof
