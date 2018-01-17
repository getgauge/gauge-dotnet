$runnerManifest = Get-Content .\Runner\csharp-netstandard.json | Out-String | ConvertFrom-Json
$version = $runnerManifest.version

& gauge uninstall csharp-netstandard -v $version
