/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Text;
using Gauge.Dotnet.Exceptions;
using Gauge.Dotnet.Loaders;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Gauge.Dotnet.UnitTests.Loaders;

[TestFixture]
internal class AssemblyLocaterTests
{
    private string _gaugeBinDir;

    [SetUp]
    public void Setup()
    {
        _gaugeBinDir = Path.Combine(Directory.GetCurrentDirectory(), "gauge_bin");
        Directory.CreateDirectory(_gaugeBinDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_gaugeBinDir))
            Directory.Delete(_gaugeBinDir);
    }

    [Test]
    public void ShouldGetAssembliesFromGaugeBin()
    {
        var expected = "Mock.Test";
        var fileInfoMock = new Mock<IFileInfo>();
        fileInfoMock.Setup(_ => _.Name).Returns($"{expected}.deps.json");
        fileInfoMock.Setup(_ => _.PhysicalPath).Returns($"{expected}.deps.json");
        var directoryContentsMock = new Mock<IDirectoryContents>();
        directoryContentsMock.Setup(_ => _.GetEnumerator()).Returns((new List<IFileInfo> { fileInfoMock.Object }).GetEnumerator());
        var fileProviderMock = new Mock<IFileProvider>();
        fileProviderMock.Setup(_ => _.GetDirectoryContents(string.Empty)).Returns(directoryContentsMock.Object);

        var assembly = AssemblyLocater.GetTestAssembly(fileProviderMock.Object);

        Assert.That(assembly, Is.EqualTo($"{expected}.dll"));
    }

    [Test]
    public void ShouldNotAddAssembliesFromInvalidFile()
    {
        var fileProvider = new PhysicalFileProvider(_gaugeBinDir);

        var expected = $"Could not locate the target test assembly. Gauge-Dotnet could not find a deps.json file in {_gaugeBinDir}";
        Assert.Throws<GaugeTestAssemblyNotFoundException>(() => AssemblyLocater.GetTestAssembly(fileProvider), expected);
    }

    [Test]
    public void GetAssembliesReferencingGaugeLib_ShouldGetLibsDependentOnGaugeLib_WhenFullDepsFileProvided()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonContents));
        var fileInfoMock = new Mock<IFileInfo>();
        fileInfoMock.Setup(_ => _.Name).Returns("Mock.Test.deps.json");
        fileInfoMock.Setup(_ => _.CreateReadStream()).Returns(stream);
        var directoryContentsMock = new Mock<IDirectoryContents>();
        directoryContentsMock.Setup(_ => _.GetEnumerator()).Returns((new List<IFileInfo> { fileInfoMock.Object }).GetEnumerator());
        var fileProviderMock = new Mock<IFileProvider>();
        fileProviderMock.Setup(_ => _.GetDirectoryContents(string.Empty)).Returns(directoryContentsMock.Object);
        var loggerMock = new Mock<ILogger>();

        var assemblies = AssemblyLocater.GetAssembliesReferencingGaugeLib(fileProviderMock.Object, loggerMock.Object).ToList();

        Assert.That(assemblies, Has.Count.EqualTo(3));
        Assert.That(assemblies, Does.Contain("Mock.Test.dll"));
        Assert.That(assemblies, Does.Contain("Does.Contain.Lib.dll"));
        Assert.That(assemblies, Does.Contain("Multiple.Contain.Lib.dll"));
    }

    [Test]
    public void GetAssembliesReferencingGaugeLib_ShouldDefaultToFileName_WhenParsingDepsFileFails()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{ }"));
        var fileInfoMock = new Mock<IFileInfo>();
        fileInfoMock.Setup(_ => _.Name).Returns("Mock.Test.deps.json");
        fileInfoMock.Setup(_ => _.CreateReadStream()).Returns(stream);
        var directoryContentsMock = new Mock<IDirectoryContents>();
        directoryContentsMock.Setup(_ => _.GetEnumerator()).Returns((new List<IFileInfo> { fileInfoMock.Object }).GetEnumerator());
        var fileProviderMock = new Mock<IFileProvider>();
        fileProviderMock.Setup(_ => _.GetDirectoryContents(string.Empty)).Returns(directoryContentsMock.Object);
        var loggerMock = new Mock<ILogger>();

        var assemblies = AssemblyLocater.GetAssembliesReferencingGaugeLib(fileProviderMock.Object, loggerMock.Object).ToList();

        Assert.That(assemblies, Has.Count.EqualTo(1));
        Assert.That(assemblies, Does.Contain("Mock.Test.dll"));
    }

    [Test]
    public void GetAssembliesReferencingGaugeLib_ShouldThrowNotFoundException_WhenDepsFileNotFound()
    {
        var fileProvider = new PhysicalFileProvider(_gaugeBinDir);

        var exception = Assert.Throws<GaugeTestAssemblyNotFoundException>(() => AssemblyLocater.GetAssembliesReferencingGaugeLib(fileProvider, null));

        var expected = $"Could not locate the target test assembly. Gauge-Dotnet could not find a deps.json file in {_gaugeBinDir}";
        Assert.That(exception.Message, Does.Contain(expected));
    }

    private static string JsonContents => """
            {
                "targets": { 
                    "Platform,version=vX.X": { 
                        "Mock.Test/1.0.0": { 
                            "dependencies": { 
                                "Gauge.CSharp.Lib": "0.1.1" 
                            } 
                        },
                        "Does.Not.Contain.Lib/2.0.1": {
                            "dependencies": {
                                "Some.OtherLib": "1.1.1"
                            },
                            "runtime": {
                                "some/path/Does.Not.Contain.Lib.dll": "version"
                            }
                        },
                        "Does.Not.Contain.Deps/5.1.1": {
                            "runtime": {
                                "some/path/Does.Not.Contain.Deps.dll": "version"
                            }
                        },
                        "Does.Contain.Lib/8.0.1": {
                            "dependencies": {
                                "Gauge.CSharp.Lib": "0.1.1"
                            },
                            "runtime": {
                                "some/path/Does.Contain.Lib.dll": "version"
                            }
                        },
                        "Multiple.Contain.Lib/2.0.1": {
                            "dependencies": {
                                "Some.OtherLib": "1.1.1",
                                "Gauge.CSharp.Lib": "0.1.1",
                                "Yet.Some.OtherLib": "2.1.1"
                            },
                            "runtime": {
                                "some/path/Multiple.Contain.Lib.dll": "version"
                            }
                        }
                    } 
                } 
            }
        """;
}