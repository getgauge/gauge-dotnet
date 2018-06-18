# Gauge-dotnet
[![Build Status](https://travis-ci.org/getgauge/gauge-dotnet.svg?branch=master)](https://travis-ci.org/getgauge/gauge-dotnet)
[![Build status](https://ci.appveyor.com/api/projects/status/t8hjbilxmb6enn4d/branch/master?svg=true)](https://ci.appveyor.com/project/getgauge/gauge-dotnet/branch/master)
[![Download Nightly](https://api.bintray.com/packages/gauge/gauge-dotnet/Nightly/images/download.svg) ](https://bintray.com/gauge/gauge-dotnet/Nightly/_latestVersion)



This project adds dotnet-core [language plugin](https://docs.gauge.org/latest/installation.html#language-runner) for [gauge](http://gauge.org).

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
gauge install dotnet --version 0.0.1
```

#### Offline installation

* Download the plugin from [Releases](https://github.com/getgauge/gauge-dotnet/releases)
```
gauge install dotnet --file gauge-dotnet-0.0.1.zip
```

#### Nightly installation

To install dotnet nightly, download the latest nightly from [here](https://bintray.com/gauge/gauge-dotnet/Nightly).

Once you have the downloaded nightly gauge-dotnet-version.nightly-yyyy-mm-dd.zip, install using:

    gauge install dotnet -f gauge-dotnet-version.nightly-yyyy-mm-dd.zip


#### Build from Source

##### Requirements
* [Dotnet Core](https://docs.microsoft.com/en-us/dotnet/core/index)
* [Gauge](http://getgauge.io)
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

![GNU Public License version 3.0](http://www.gnu.org/graphics/gplv3-127x51.png)
Gauge-Dotnet is released under [GNU Public License version 3.0](http://www.gnu.org/licenses/gpl-3.0.txt)

## Copyright

Copyright 2018 ThoughtWorks, Inc.
