$runnerManifest = Get-Content .\src\dotnet.json | Out-String | ConvertFrom-Json
$version = $runnerManifest.version

Add-Type -Assembly "System.IO.Compression.FileSystem" ;
[System.IO.Compression.ZipFile]::CreateFromDirectory(".\deploy", ".\artifacts\gauge-dotnet-$version.zip")