// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.UnitTests.Helpers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Extensions
{
    internal class MethodInfoExtensionTests
    {
        //[Step("Foo")]
        //public void Foo()
        //{
        //}

        //[Step("Bar")]
        //[ContinueOnFailure]
        //public void Bar(string bar)
        //{
        //}

        //[ContinueOnFailure]
        //public void Baz(string bar)
        //{
        //}

        [Test]
        public void ShouldGetFullyQualifiedName()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            var fooMethod = new MockMethodBuilder(assemblyLoader)
                .WithName("Foo")
                .WithDeclaringTypeName("My.Test.Type")
                .WithStep("Foo")
                .Build();

            Assert.AreEqual("My.Test.Type.Foo", fooMethod.FullyQuallifiedName());
        }

        [Test]
        public void ShouldGetFullyQualifiedNameWithParams()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            var barMethod = new MockMethodBuilder(assemblyLoader)
                .WithName("Bar")
                .WithStep("Bar")
                .WithDeclaringTypeName("My.Test.Type")
                .WithContinueOnFailure()
                .WithParameters(new KeyValuePair<string, string>("String", "bar"))
                .Build();

            Assert.AreEqual("My.Test.Type.Bar-Stringbar", barMethod.FullyQuallifiedName());
        }

        [Test]
        public void ShouldNotBeRecoverable()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            var fooMethod = new MockMethodBuilder(assemblyLoader)
                .WithName("Foo")
                .WithStep("Foo")
                .Build();

            Assert.False(fooMethod.IsRecoverableStep(assemblyLoader.Object));
        }

        [Test]
        public void ShouldBeRecoverableWhenContinueOnFailure()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            var barMethod = new MockMethodBuilder(assemblyLoader)
                .WithName("Bar")
                .WithStep("Bar")
                .WithContinueOnFailure()
                .WithParameters(new KeyValuePair<string, string>("string", "Bar"))
                .Build();

            Assert.True(barMethod.IsRecoverableStep(assemblyLoader.Object));
        }

        [Test]
        public void ShouldNotBeRecoverableWhenContinueOnFailureOnNonStep()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            var bazMethod = new MockMethodBuilder(assemblyLoader)
                .WithName("Baz")
                .WithContinueOnFailure()
                .WithParameters(new KeyValuePair<string, string>("string", "Bar"))
                .Build();

            Assert.False(bazMethod.IsRecoverableStep(assemblyLoader.Object),
                "Recoverable is true only when method is a Step");
        }
    }
}