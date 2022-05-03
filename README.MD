# Gauge-dotnet
[![tests](https://github.com/getgauge/gauge-dotnet/workflows/tests/badge.svg)](https://github.com/getgauge/gauge-dotnet/actions?query=workflow%3Atests)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v1.4%20adopted-ff69b4.svg)](CODE_OF_CONDUCT.md)


This project adds dotnet-core [language plugin](https://gauge.org/plugins/) for [gauge](https://gauge.org).

The plugin is authored in [c#](https://en.wikipedia.org/wiki/C_Sharp_(programming_language)).

## Getting started

### Pre-requisite

- [Gauge](https://gauge.org)
- [Dotnet Core](https://docs.microsoft.com/en-us/dotnet/core/index)


### Installation

```
gauge install dotnet
```

### Create a gauge-dotnet project

```
gauge init dotnet
```

### Run tests

```
gauge run specs
```

### Alternate Installation options

#### Install specific version
* Installing specific version
```
gauge install dotnet --version 0.5.0
```

#### Offline installation

* Download the plugin from [Releases](https://github.com/getgauge/gauge-dotnet/releases)
```
gauge install dotnet --file gauge-dotnet-0.5.0.zip
```

#### Build from Source

##### Requirements
* [Dotnet Core](https://docs.microsoft.com/en-us/dotnet/core/index)
* [Gauge](https://gauge.org/)
* [JQ](https://stedolan.github.io/jq/) (for unix)


Running `run.sh` or `run.cmd` should give the list of all tasks available. Below sections detail some commonly used tasks.

##### Compiling

To build the project dlls:

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

To install the the dotnet plugin use(Note, this will uninstall gauge-dotnet before installing the compiled version):

````
./run.cmd|./run.sh forceinstall
````

##### Creating distributable

````
./run.cmd|./run.sh package
````

New distribution details need to be updated in the dotnet-install.json file in  [gauge plugin repository](https://github.com/getgauge/gauge-repository) for a new verison update.

## License

This program is licensed under:

Apache License, Version 2.0

## Copyright

Copyright 2018 ThoughtWorks, Inc.
