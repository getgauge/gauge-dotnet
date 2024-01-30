/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Wrappers;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class StaticLoaderTests
    {
        private string oldEnv;
        private readonly string dummyProjectRoot = Path.Combine("non", "existent", "path");
        [SetUp]
        public void Setup()
        {
            oldEnv = Environment.GetEnvironmentVariable("GAUGE_PROJECT_ROOT");
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", dummyProjectRoot);
        }

        [TearDown]
        public void Teardown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", oldEnv);
        }

        [Test]
        public void ShouldAddAliasesSteps()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
            mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
            var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object);
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
            var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object);

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
            var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object);
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
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            var csprojText = XDocument.Parse("<Compile Remove=\"foo.cs\" />");
            var attributes = csprojText.Descendants().Attributes("Remove");
            var list = new List<XAttribute>();
            list.AddRange(attributes);
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(list);
            var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
            var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object);

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
            var filePath = Path.Combine(Utils.GaugeProjectRoot, fileName);
            loader.ReloadSteps(text, filePath);
            ClassicAssert.False(loader.GetStepRegistry().ContainsStep("hello"));
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Test]
        public void ShouldReloadSteps()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
            mockDirectoryWrapper.Setup(x => x.EnumerateFiles(dummyProjectRoot, "*.cs", SearchOption.AllDirectories)).Returns(Enumerable.Empty<string>);
            var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object);
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
            var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object);
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

        public class LoadImplementationsTest
        {
            private const string GaugeCustomBuildPathEnv = "GAUGE_CUSTOM_BUILD_PATH";
            private string old;

            [SetUp]
            public void Setup()
            {
                old = Utils.TryReadEnvValue(GaugeCustomBuildPathEnv);
                Environment.SetEnvironmentVariable(GaugeCustomBuildPathEnv, "foo");
            }

            [Test]
            public void ShouldNotLoadWhenCustomBuildPathIsSet()
            {
                var mockAttributesLoader = new Mock<IAttributesLoader>();
                mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Verifiable();
                var mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
                mockDirectoryWrapper.Setup(x => x.EnumerateFiles(It.IsAny<string>(), "*.cs", SearchOption.AllDirectories));

                var loader = new StaticLoader(mockAttributesLoader.Object, mockDirectoryWrapper.Object);

                mockAttributesLoader.Verify(x => x.GetRemovedAttributes(), Times.Never());
                Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", old);
            }

            [TearDown]
            public void TearDown()
            {
                Environment.SetEnvironmentVariable(GaugeCustomBuildPathEnv, old);
            }
        }
    }
}