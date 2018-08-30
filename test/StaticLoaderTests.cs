// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-Dotnet.
//
// Gauge-Dotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-Dotnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-Dotnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gauge.CSharp.Core;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class StaticLoaderTests
    {
        [Test]
        public void ShouldAddAliasesSteps()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(mockAttributesLoader.Object);
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

            Assert.True(registry.ContainsStep("goodbye"));
            Assert.True(registry.ContainsStep("adieu"));
            Assert.True(registry.ContainsStep("sayonara"));
        }

        [Test]
        public void ShouldAddStepsFromGivenContent()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(mockAttributesLoader.Object);

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
            Assert.True(loader.GetStepRegistry().ContainsStep("hello"));
        }

        [Test]
        public void ShouldLoadStepsWithPositoin()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(mockAttributesLoader.Object);
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
            Assert.AreEqual(1, positions.Count);
            Assert.AreEqual(6, positions.First().Span.Start);
            Assert.AreEqual(9, positions.First().Span.End);
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
            var loader = new StaticLoader(mockAttributesLoader.Object);

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
            Assert.False(loader.GetStepRegistry().ContainsStep("hello"));
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Test]
        public void ShouldReloadSteps()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(mockAttributesLoader.Object);
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
            Assert.True(loader.GetStepRegistry().ContainsStep("hello"));

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

            Assert.True(loader.GetStepRegistry().ContainsStep("hola"));
        }

        [Test]
        public void ShouldRemoveSteps()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(mockAttributesLoader.Object);
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

            Assert.True(loader.GetStepRegistry().ContainsStep("hello"));
            Assert.True(loader.GetStepRegistry().ContainsStep("hola"));

            loader.RemoveSteps(file2);

            Assert.False(loader.GetStepRegistry().ContainsStep("hola"));
        }
    }
}