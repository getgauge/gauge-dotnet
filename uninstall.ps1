$runnerManifest = Get-Content .\src\dotnet.json | Out-String | ConvertFrom-Json
$version = $runnerManifest.version

& gauge uninstall dotnet -v $version
