#!/bin/sh

command -v $dotnet >/dev/null 2>&1 || { echo >&2 "dotnet is not installed, aborting."; exit 1; }

case "$(dotnet --version)" in
  6.*) dotnet bin/net6.0/Gauge.Dotnet.dll $@ ;;
  7.*) dotnet bin/net7.0/Gauge.Dotnet.dll $@ ;;
  *) dotnet bin/net8.0/Gauge.Dotnet.dll $@ ;;
esac
