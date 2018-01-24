$runnerManifest = Get-Content .\Runner\dotnet.json | Out-String | ConvertFrom-Json
$version = $runnerManifest.version

& gauge install dotnet -f ".\artifacts\gauge-dotnet-$version.zip"
