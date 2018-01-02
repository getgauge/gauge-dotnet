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

using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Extensions;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests.Extensions
{
    public class MethodInfoExtensionTests
    {
        [Step("Foo")]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Foo()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [Step("Bar")]
        [ContinueOnFailure]
#pragma warning disable xUnit1013 // Public method should be marked as test
#pragma warning disable RECS0154 // Parameter is never used
        public void Bar(string bar)
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }


        [ContinueOnFailure]
#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Baz(string bar)
#pragma warning restore xUnit1013 // Public method should be marked as test
#pragma warning restore RECS0154 // Parameter is never used
        {
        }

        [Theory]
        [InlineData("Foo", "Gauge.CSharp.Runner.UnitTests.Extensions.MethodInfoExtensionTests.Foo")]
        [InlineData("Bar", "Gauge.CSharp.Runner.UnitTests.Extensions.MethodInfoExtensionTests.Bar-Stringbar")]
        public void ShouldGetFullyQualifiedName(string methodName, string expectedMethodId)
        {
            Assert.Equal(expectedMethodId, GetType().GetMethod(methodName).FullyQuallifiedName());
        }

        [Theory]
        [InlineData("Foo", false)]
        [InlineData("Bar", true)]
        // [InlineData("Baz", false, Description = "Recoverable is true only when method is a Step")]
        [InlineData("Baz", false)]
        public void ShouldGetRecoverable(string methodName, bool expectedRecoverable)
        {
            Assert.Equal(expectedRecoverable, GetType().GetMethod(methodName).IsRecoverableStep());
        }
    }
}