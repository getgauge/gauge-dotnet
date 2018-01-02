// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using Gauge.CSharp.Runner.Extensions;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class CSharpIdentifierTests
    {
        [InlineData("With Spaces", "WithSpaces", true)]
        [InlineData("Special*chars%!", "SpecialChars", true)]
        [InlineData("   begins with whitespace", "BeginsWithWhitespace", true)]
        [InlineData("ends with whitespace   ", "EndsWithWhitespace", true)]
        [InlineData("class", "Class", true)]
        [InlineData("class", "@class", false)]
        [InlineData("int", "Int", true)]
        [InlineData("abstract", "Abstract", true)]
        [InlineData("foo", "foo", true)]
        [InlineData("foo", "foo", false)]
        [Theory]
        public void GeneratesValidIdentifiers(string input, string expected, bool camelCase)
        {
            Assert.Equal(expected, input.ToValidCSharpIdentifier(camelCase));
        }
    }
}