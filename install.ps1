$runnerManifest = Get-Content .\src\dotnet.json | Out-String | ConvertFrom-Json
$version = $runnerManifest.version

& gauge install dotnet -f ".\artifacts\gauge-dotnet-$version.zip"
