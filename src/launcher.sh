#!/bin/sh

command -v $dotnet >/dev/null 2>&1 || { echo >&2 "dotnet is not installed, aborting."; exit 1; }

case "$(dotnet --version)" in
  3.*) dotnet bin/netcoreapp3.0/Gauge.Dotnet.dll $@ ;;
  *) dotnet bin/net5.0/Gauge.Dotnet.dll $@ ;;
esac
