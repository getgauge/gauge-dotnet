#!/bin/sh

command -v $dotnet >/dev/null 2>&1 || { echo >&2 "dotnet is not installed, aborting."; exit 1; }

if [[ "$(dotnet --version)" == *"3."* ]]; then
    dotnet bin/netcoreapp3.0/Gauge.Dotnet.dll $@
else
    dotnet bin/net5.0/Gauge.Dotnet.dll $@
fi

