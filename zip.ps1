$runnerManifest = Get-Content .\Runner\csharp-netstandard.json | Out-String | ConvertFrom-Json
$version = $runnerManifest.version

Add-Type -Assembly "System.IO.Compression.FileSystem" ;
[System.IO.Compression.ZipFile]::CreateFromDirectory(".\deploy", ".\artifacts\gauge-csharp-netstandard-$version.zip")