# ----------------------------------------------------------------
#   Copyright (c) ThoughtWorks, Inc.
#   Licensed under the Apache License, Version 2.0
#   See LICENSE.txt in the project root for license information.
# ----------------------------------------------------------------

dotnet restore

$grpc_tools_version = "2.71.0"
$grpc_tools = Join-Path $HOME ".nuget\packages\grpc.tools\$grpc_tools_version\tools"

$protoc = $null
$grpc_csharp = $null

if ($env:PROCESSOR_ARCHITECTURE -match 64){
    $protoc = Resolve-Path $grpc_tools\windows_x64\protoc.exe
    $grpc_csharp = Resolve-Path $grpc_tools\windows_x64\grpc_csharp_plugin.exe
}
else {
    $protoc = Resolve-Path $grpc_tools\windows_x86\protoc.exe
    $grpc_csharp = Resolve-Path $grpc_tools\windows_x86\grpc_csharp_plugin.exe
}

Write-Host "Generating Proto Classes.."


gci ".\gauge-proto" -Filter "*.proto" | %{
    Write-Host "Generating classes for $_"
    &$protoc @('-I.\gauge-proto', '--csharp_out=.\src\Gauge.CSharp.Core','--grpc_out=.\src\Gauge.CSharp.Core',"--plugin=protoc-gen-grpc=$grpc_csharp", ".\gauge-proto\$_")
}

Write-Host "Done!"
