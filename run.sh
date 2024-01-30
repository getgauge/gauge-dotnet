#!/bin/bash

function checkCommand() {
    command -v $1 >/dev/null 2>&1 || { echo >&2 "$1 is not installed, aborting."; exit 1; }
}

function build() {
    checkCommand "dotnet"
    dotnet build -c Release
}

function test() {
    checkCommand "dotnet"
    dotnet test --no-build -c Release test/Gauge.Dotnet.UnitTests.csproj
    
    # Only the last command's exit code is honored unless we exit early
    testExit=$?
    if [ $testExit -gt 0 ]; then exit $testExit; fi
    
    dotnet test --no-build -c Release integration-test/Gauge.Dotnet.IntegrationTests.csproj
}

function version() {
    checkCommand "jq"
    echo `cat src/dotnet.json | jq -r .version`
}

function package() {
    checkCommand "dotnet"
    checkCommand "zip"
    rm -rf deploy artifacts
    dotnet publish -c release -o ./deploy/bin/net6.0 src/Gauge.Dotnet.csproj -f net6.0
    dotnet publish -c release -o ./deploy/bin/net7.0 src/Gauge.Dotnet.csproj -f net7.0
    dotnet publish -c release -o ./deploy/bin/net8.0 src/Gauge.Dotnet.csproj -f net8.0
    cp src/launcher.sh deploy
    cp src/launcher.cmd deploy
    cp src/dotnet.json deploy
    mkdir -p artifacts
    (export version=$(version) && cd deploy && zip -r ../artifacts/gauge-dotnet-$version.zip .)
}

function install() {
    package
    gauge install dotnet -f ./artifacts/gauge-dotnet-$(version).zip
}

function uninstall() {
    gauge uninstall dotnet -v $(version)
}

function forceinstall() {
    uninstall
    install
}

tasks=(build test package install uninstall forceinstall)
if [[ " ${tasks[@]} " =~ " $1 " ]]; then
    $1
    exit $?
fi

echo Options: [build \| test \| package \| install \| uninstall \| forceinstall]

