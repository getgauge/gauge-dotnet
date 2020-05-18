/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Extensions;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class CSharpIdentifierTests
    {
        [TestCase("With Spaces", "WithSpaces", true)]
        [TestCase("Special*chars%!", "SpecialChars", true)]
        [TestCase("   begins with whitespace", "BeginsWithWhitespace", true)]
        [TestCase("ends with whitespace   ", "EndsWithWhitespace", true)]
        [TestCase("class", "Class", true)]
        [TestCase("class", "@class", false)]
        [TestCase("int", "Int", true)]
        [TestCase("abstract", "Abstract", true)]
        [TestCase("foo", "foo", true)]
        [TestCase("foo", "foo", false)]
        [Test]
        public void GeneratesValidIdentifiers(string input, string expected, bool camelCase)
        {
            Assert.AreEqual(expected, input.ToValidCSharpIdentifier(camelCase));
        }
    }
}