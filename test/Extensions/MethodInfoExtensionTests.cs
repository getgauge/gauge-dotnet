/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.UnitTests.Helpers;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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

            ClassicAssert.AreEqual("My.Test.Type.Foo", fooMethod.FullyQuallifiedName());
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

            ClassicAssert.AreEqual("My.Test.Type.Bar-Stringbar", barMethod.FullyQuallifiedName());
        }

        [Test]
        public void ShouldNotBeRecoverable()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            var fooMethod = new MockMethodBuilder(assemblyLoader)
                .WithName("Foo")
                .WithStep("Foo")
                .Build();

            ClassicAssert.False(fooMethod.IsRecoverableStep(assemblyLoader.Object));
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

            ClassicAssert.True(barMethod.IsRecoverableStep(assemblyLoader.Object));
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

            ClassicAssert.False(bazMethod.IsRecoverableStep(assemblyLoader.Object),
                "Recoverable is true only when method is a Step");
        }
    }
}