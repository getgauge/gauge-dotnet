# Gauge-dotnet

[![tests](https://github.com/getgauge/gauge-dotnet/actions/workflows/build.yml/badge.svg)](https://github.com/getgauge/gauge-dotnet/actions/workflows/build.yml)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v1.4%20adopted-ff69b4.svg)](CODE_OF_CONDUCT.md)

This project adds a .NET [language plugin](https://gauge.org/plugins/) for [Gauge](https://gauge.org).

The plugin is authored in [C#](https://en.wikipedia.org/wiki/C_Sharp_(programming_language)).

## Getting started

### Pre-requisites

- [Gauge](https://gauge.org/)
- [.NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/)

### Installation

```
gauge install dotnet
```

### Create a Gauge .NET project

```
gauge init dotnet
```

### Run tests

```
gauge run specs
```

### Alternate Installation options

#### Install specific version

```
gauge install dotnet --version 0.6.0
```

#### Offline installation

Download the plugin from [Releases](https://github.com/getgauge/gauge-dotnet/releases)

```
gauge install dotnet --file gauge-dotnet-0.5.8.zip
```

#### Build from Source

##### Requirements
* [.NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/)
* [Gauge](https://gauge.org/)
* [JQ](https://stedolan.github.io/jq/) (for unix)

Running `run.sh` or `run.cmd` should give the list of all tasks available. Below sections detail some commonly used tasks.

##### Compiling

To build the project DLLs:

````
./run.cmd|./run.sh build
````

##### Testing

After compiling

To run tests (unit and integration):

````
./run.cmd|./run.sh test
````

##### Installing

To install the dotnet plugin use (note, this will uninstall `gauge-dotnet` before installing the compiled version):

````
./run.cmd|./run.sh forceinstall
````

##### Creating distributable

````
./run.cmd|./run.sh package
````

New distribution details need to be updated in the `dotnet-install.json` file in the [gauge plugin repository](https://github.com/getgauge/gauge-repository) for a new version update.

## License

This program is licensed under:

Apache License, Version 2.0

## Copyright

Copyright 2018 ThoughtWorks, Inc.
