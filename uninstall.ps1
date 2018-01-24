$runnerManifest = Get-Content .\Runner\dotnet.json | Out-String | ConvertFrom-Json
$version = $runnerManifest.version

& gauge uninstall dotnet -v $version
