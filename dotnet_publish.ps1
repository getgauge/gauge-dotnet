$version = $(dotnet --version)
if ($version.Contains("3")) {
    dotnet publish -c release -o .\deploy\bin src\Gauge.Dotnet.csproj
}
else {
    dotnet publish -c release -o ..\deploy\bin src\Gauge.Dotnet.csproj
}
