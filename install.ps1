$runnerManifest = Get-Content .\Runner\csharp-netstandard.json | Out-String | ConvertFrom-Json
$version = $runnerManifest.version

& gauge install csharp-netstandard -f ".\artifacts\gauge-csharp-netstandard-$version.zip"
