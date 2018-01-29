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

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gauge.Dotnet.UnitTests.Helpers
{
    public class MockMethodBuilder
    {
        private Mock<MethodInfo> mockMethod;
        private readonly Mock<IAssemblyLoader> mockAssemblyLoader;
        private List<object> methodAttributes;
        private Mock<Type> mockContinueOnFailureType;
        private Mock<Type> mockStepType;

        private class TestFilteredHook
        {
            public string[] FilterTags { get; set; }
        }

        private class TestTagAggregation
        {
            public int TagAggregation { get; set; }
        }

        private class TestStep
        {
            public IEnumerable<string> Names { get; set; }
        }

        private class TestContinueOnFailure { }
        public MockMethodBuilder(Mock<IAssemblyLoader> mockAssemblyLoader)
        {
            mockMethod = new Mock<MethodInfo>();
            this.mockAssemblyLoader = mockAssemblyLoader;
            methodAttributes = new List<object>();
            mockContinueOnFailureType = new Mock<Type>();
            mockStepType = new Mock<Type>();
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ContinueOnFailure)).Returns(mockContinueOnFailureType.Object);
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.Step)).Returns(mockStepType.Object);
        }

        internal MockMethodBuilder WithDeclaringTypeName(string name)
        {
            var declaringType = new Mock<Type>();
            declaringType.Setup(x => x.FullName).Returns(name);
            mockMethod.Setup(x => x.DeclaringType).Returns(declaringType.Object);
            return this;
        }

        public MethodInfo Build()
        {
            mockMethod.Setup(x => x.GetCustomAttributes(false)).Returns(methodAttributes.ToArray());
            return mockMethod.Object;
        }

        public MockMethodBuilder WithName(string name)
        {
            mockMethod.Setup(x => x.Name).Returns(name);
            return this;
        }

        public MockMethodBuilder WithParameters(params KeyValuePair<string, string>[] methodParams)
        {
            mockMethod.Setup(x => x.GetParameters()).Returns(GetParameters(methodParams));
            return this;
        }

        public MockMethodBuilder WithStep(params string[] stepTexts)
        {
            var step = new TestStep { Names = stepTexts };
            mockStepType.Setup(x => x.IsInstanceOfType(step)).Returns(true);
            methodAttributes.Add(step);
            return this;
        }

        public MockMethodBuilder WithContinueOnFailure()
        {
            var continueOnFailure = new TestContinueOnFailure();
            mockContinueOnFailureType.Setup(x => x.IsInstanceOfType(continueOnFailure)).Returns(true);
            methodAttributes.Add(continueOnFailure);
            return this;
        }

        public MockMethodBuilder WithFilteredHook(LibType hook, params string[] tags)
        {
            var mockFilteredHook = new Mock<Type>();
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.FilteredHookAttribute)).Returns(mockFilteredHook.Object);

            var mockHookType = new Mock<Type>();
            mockHookType.Setup(x => x.IsSubclassOf(mockFilteredHook.Object)).Returns(true);
            var hookAttribute = new TestFilteredHook { FilterTags = tags.Length > 0 ? tags: null };
            mockHookType.Setup(x => x.IsInstanceOfType(It.IsAny<TestFilteredHook>())).Returns(true);
            mockAssemblyLoader.Setup(a => a.GetLibType(hook)).Returns(mockHookType.Object);

            methodAttributes.Add(hookAttribute);

            var mockTagAggregationBehaviourType = new Mock<Type>();
            mockTagAggregationBehaviourType.Setup(x => x.IsInstanceOfType(It.IsAny<TestTagAggregation>())).Returns(true);
            mockAssemblyLoader.Setup(a => a.GetLibType(LibType.TagAggregationBehaviourAttribute)).Returns(mockTagAggregationBehaviourType.Object);
            return this;
        }

        public MockMethodBuilder WithTagAggregation(int aggregation)
        {
            methodAttributes.Add(new TestTagAggregation { TagAggregation = aggregation });
            return this;
        }

        static ParameterInfo[] GetParameters(IEnumerable<KeyValuePair<string, string>> methodParams)
        {
            if (methodParams is null)
                return Enumerable.Empty<ParameterInfo>().ToArray();
            return methodParams.Select(p =>
            {
                var pInfo = new Mock<ParameterInfo>();
                var pType = new Mock<Type>();
                pType.Setup(x => x.Name).Returns(p.Key);
                pInfo.Setup(x => x.ParameterType).Returns(pType.Object);
                pInfo.Setup(x => x.Name).Returns(p.Value);
                return pInfo.Object;
            }).ToArray();
        }
    }
}
