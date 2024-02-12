#! /bin/bash

set_os_architecture() {
    echo "Detecting architecture"
    local _ostype="$(uname -s)"
    local _cputype="$(uname -m)"

    echo "uname -s reports: $_ostype"
    echo "uname -m reports: $_cputype"

    if [ "$_ostype" = Darwin -a "$_cputype" = i386 ]; then
        # Darwin `uname -s` lies
        if sysctl hw.optional.x86_64 | grep -q ': 1'; then
            local _cputype=x86_64
        fi
    fi

    case "$_ostype" in

        Linux)
            local _ostype=linux
            ;;
        Darwin)
            local _ostype=macosx
            ;;
        *)
            #err "Unknown OS type: $_ostype"
            ;;
    esac

    case "$_cputype" in

        i386 | i486 | i686 | i786 | x86)
            local _cputype=x86
            ;;
        x86_64 | x86-64 | x64 | amd64)
            local _cputype=x64
            ;;
        *)
            #err "Unknown CPU type: $_cputype"
            ;;
    esac

    echo "OS is $_ostype"
    echo "Architecture is $_cputype"
    ARCH="$_cputype"
    OS="$_ostype"
}


set_os_architecture

dotnet restore

grpc_tools_version="2.61.0"
protoc="$HOME"/.nuget/packages/build/grpc.tools/"grpc_tools_version"/tools/"$OS"_"$ARCH"/protoc
grpc_csharp="$HOME"/.nuget/packages/build/grpc.tools/"grpc_tools_version"/tools/"$OS"_"$ARCH"/grpc_csharp_plugin

chmod +x $protoc
chmod +x $grpc_csharp

echo "Generating Proto Classes.."

for i in `ls ./gauge-proto/*.proto`; do
    $protoc -I./gauge-proto --csharp_out=./src/Gauge.CSharp.Core --grpc_out=./src/Gauge.CSharp.Core --plugin=protoc-gen-grpc=$grpc_csharp $i
done

echo "Done"