#!/bin/sh

command -v $dotnet >/dev/null 2>&1 || { echo >&2 "dotnet is not installed, aborting."; exit 1; }

case "$(dotnet --version)" in
  8.*) dotnet bin/net8.0/Gauge.Dotnet.dll $@ ;;
  *) dotnet bin/net9.0/Gauge.Dotnet.dll $@ ;;
esac
