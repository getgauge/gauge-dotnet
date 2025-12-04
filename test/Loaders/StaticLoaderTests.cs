/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Xml.Linq;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gauge.Dotnet.UnitTests.Loaders;

[TestFixture]
public class StaticLoaderTests
{
    private IConfiguration _config;
    private readonly string dummyProjectRoot = Path.Combine("non", "existent", "path");
    private readonly Mock<ILogger<StaticLoader>> _logger = new();

    [SetUp]
    public void Setup()
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { { "GAUGE_PROJECT_ROOT", dummyProjectRoot } })
            .Build();
    }

    [Test]
    public void ShouldAddAliasesSteps()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);
        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Step(\"goodbye\",\"adieu\", \"sayonara\")]\n" +
                            "        public void farewell()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        var registry = loader.GetStepRegistry();

        ClassicAssert.True(registry.ContainsStep("goodbye"));
        ClassicAssert.True(registry.ContainsStep("adieu"));
        ClassicAssert.True(registry.ContainsStep("sayonara"));
    }

    [Test]
    public void ShouldAddStepsFromGivenContent()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));
    }

    [Test]
    public void ShouldLoadStepsWithPosition()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);
        const string file1 = @"Foo.cs";

        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class Foo\n" +
                            "    {\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";

        loader.LoadStepsFromText(text, file1);

        const string file2 = @"Bar.cs";
        const string newText = "using Gauge.CSharp.Lib.Attributes;\n" +
                               "namespace foobar\n" +
                               "{\n" +
                               "    public class Bar\n" +
                               "    {\n" +
                               "        [Step(\"hola\")]\n" +
                               "        public void hola()\n" +
                               "        {\n" +
                               "        }\n" +
                               "    }\n" +
                               "}\n";

        loader.ReloadSteps(newText, file2);

        var positions = loader.GetStepRegistry().GetStepPositions(file1).ToList();
        ClassicAssert.AreEqual(1, positions.Count);
        ClassicAssert.AreEqual(6, positions.First().Span.Start);
        ClassicAssert.AreEqual(9, positions.First().Span.End);
    }

    [Test]
    public void ShouldNotReloadStepOfRemovedFile()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        _config["GAUGE_PROJECT_ROOT"] = currentDirectory;
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        var csprojText = XDocument.Parse("<Compile Remove=\"foo.cs\" />");
        var attributes = csprojText.Descendants().Attributes("Remove");
        var list = new List<XAttribute>();
        list.AddRange(attributes);
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(list);
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        var filePath = Path.Combine(currentDirectory, fileName);
        loader.ReloadSteps(text, filePath);
        ClassicAssert.False(loader.GetStepRegistry().ContainsStep("hello"));
    }

    [Test]
    public void ShouldReloadSteps()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);
        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));

        const string newText = "using Gauge.CSharp.Lib.Attributes;\n" +
                               "namespace foobar\n" +
                               "{\n" +
                               "    public class FooBar\n" +
                               "    {\n" +
                               "        [Step(\"hello\")]\n" +
                               "        public void hello()\n" +
                               "        {\n" +
                               "        }\n" +
                               "        [Step(\"hola\")]\n" +
                               "        public void hola()\n" +
                               "        {\n" +
                               "        }\n" +
                               "    }\n" +
                               "}\n";

        loader.ReloadSteps(newText, fileName);

        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hola"));
    }

    [Test]
    public void ShouldRemoveSteps()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);
        const string file1 = @"Foo.cs";

        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class Foo\n" +
                            "    {\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";

        loader.LoadStepsFromText(text, file1);

        const string file2 = @"Bar.cs";
        const string newText = "using Gauge.CSharp.Lib.Attributes;\n" +
                               "namespace foobar\n" +
                               "{\n" +
                               "    public class Bar\n" +
                               "    {\n" +
                               "        [Step(\"hola\")]\n" +
                               "        public void hola()\n" +
                               "        {\n" +
                               "        }\n" +
                               "    }\n" +
                               "}\n";

        loader.ReloadSteps(newText, file2);

        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hola"));

        loader.RemoveSteps(file2);

        ClassicAssert.False(loader.GetStepRegistry().ContainsStep("hola"));
    }

    [Test]
    public void ShouldFindStepAttribute_WhenUsingQualifiedName()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

        const string text = "using Gauge.CSharp.Lib;\n" +
                            "using System;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Attribute.Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));
    }

    [Test]
    public void ShouldFindStepAttribute_WhenUsingFullTypeName()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

        const string text = "using System;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Gauge.CSharp.Lib.Attribute.Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));
    }

    [Test]
    public void ShouldFindStepAttribute_WhenStepIsNotFirst()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "using System;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Obsolete]\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));
    }

    [Test]
    public void ShouldFindStepAttribute_WhenStepIsAfterMultipleAttributes()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>());
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "using System;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Obsolete]\n" +
                            "        [Test]\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));
    }

    [Test]
    public void ShouldIgnoreMethodsWithoutStepAttribute_InFileWithStepAttributes()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>());
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "using System;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "        [Obsolete]\n" +
                            "        public void nonStepMethod()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));
        ClassicAssert.AreEqual(1, loader.GetStepRegistry().Count);
    }

    [Test]
    public void ShouldHandleMethodsWithOtherAttributesButNoStep_InFileWithStepAttributes()
    {
        var mockAttributesLoader = new Mock<IAttributesLoader>();
        mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
        var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>());
        var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

        const string text = "using Gauge.CSharp.Lib.Attributes;\n" +
                            "using System;\n" +
                            "namespace foobar\n" +
                            "{\n" +
                            "    public class FooBar\n" +
                            "    {\n" +
                            "        [Step(\"hello\")]\n" +
                            "        public void hello()\n" +
                            "        {\n" +
                            "        }\n" +
                            "        [Obsolete]\n" +
                            "        [Test]\n" +
                            "        public void methodWithOtherAttributes()\n" +
                            "        {\n" +
                            "        }\n" +
                            "        public void methodWithNoAttributes()\n" +
                            "        {\n" +
                            "        }\n" +
                            "    }\n" +
                            "}\n";
        const string fileName = @"foo.cs";
        loader.LoadStepsFromText(text, fileName);
        ClassicAssert.True(loader.GetStepRegistry().ContainsStep("hello"));
        ClassicAssert.AreEqual(1, loader.GetStepRegistry().Count);
    }

    public class LoadImplementationsTest
    {
        private IConfiguration _config;
        private readonly string dummyBuildPath = Path.Combine("foo");
        private readonly Mock<ILogger<StaticLoader>> _logger = new();

        [SetUp]
        public void Setup()
        {
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "GAUGE_CUSTOM_BUILD_PATH", dummyBuildPath } })
                .Build();
        }

        [Test]
        public void ShouldNotLoadWhenCustomBuildPathIsSet()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Verifiable();
            var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
            mockDirectoryWrapper.Setup(x => x.EnumerateFiles(It.IsAny<string>(), "*.cs", SearchOption.AllDirectories));

            var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object, _config, _logger.Object);

            mockAttributesLoader.Verify(x => x.GetRemovedAttributes(), Times.Never());
        }
    }
}