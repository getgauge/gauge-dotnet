/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Linq;
using Gauge.Dotnet.Converters;
using Gauge.Messages;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests.Converter
{
    [TestFixture]
    public class StringParamConverterTests
    {
        private class TestTypeConversion
        {
            public void Int(int i)
            {
            }

            public void Float(float j)
            {
            }

            public void Bool(bool b)
            {
            }

            public void String(string s)
            {
            }
        }

        [Test]
        public void ShouldConvertFromParameterToString()
        {
            const string expected = "Foo";
            var parameter = new Parameter
            {
                ParameterType = Parameter.Types.ParameterType.Static,
                Value = expected
            };

            var actual = new StringParamConverter().Convert(parameter);

            ClassicAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ShouldTryToConvertStringParameterToBool()
        {
            var type = new TestTypeConversion().GetType();
            var method = type.GetMethod("Bool");

            var getParams = StringParamConverter.TryConvertParams(method, new object[] { "false" });
            ClassicAssert.AreEqual(typeof(bool), getParams.First().GetType());
        }

        [Test]
        public void ShouldTryToConvertStringParameterToFloat()
        {
            var type = new TestTypeConversion().GetType();
            var method = type.GetMethod("Float");

            var getParams = StringParamConverter.TryConvertParams(method, new object[] { "3.1412" });
            ClassicAssert.AreEqual(typeof(float), getParams.First().GetType());
        }

        [Test]
        public void ShouldTryToConvertStringParameterToInt()
        {
            var type = new TestTypeConversion().GetType();
            var method = type.GetMethod("Int");

            var getParams = StringParamConverter.TryConvertParams(method, new object[] { "1" });
            ClassicAssert.AreEqual(typeof(int), getParams.First().GetType());
        }

        [Test]
        public void ShouldTryToConvertStringParameterToString()
        {
            var type = new TestTypeConversion().GetType();
            var method = type.GetMethod("Int");

            var getParams = StringParamConverter.TryConvertParams(method, new object[] { "hahaha" });
            ClassicAssert.AreEqual(typeof(string), getParams.First().GetType());
        }
    }
}