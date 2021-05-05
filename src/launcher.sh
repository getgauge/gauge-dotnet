#! /bin/sh

function checkCommand() {
    command -v $1 >/dev/null 2>&1 || { echo >&2 "$1 is not installed, aborting."; exit 1; }
}

checkCommand "dotnet"

if [[ "$(dotnet --version)" == *"3."* ]]; then
    dotnet bin/netcoreapp3.0/Gauge.Dotnet.dll $@
else
    dotnet bin/net5.0/Gauge.Dotnet.dll $@
fi

