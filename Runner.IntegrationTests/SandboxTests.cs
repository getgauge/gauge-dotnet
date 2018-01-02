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
using System.Linq;
using Gauge.CSharp.Lib;
using Xunit;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    public class SandboxTests : IntegrationTestsBase
    {
        [Fact]
        public void RecoverableIsTrueOnExceptionThrownWhenContinueOnFailure()
        {
            var sandbox = new Sandbox();
            var stepMethods = sandbox.GetStepMethods();
            var gaugeMethod = stepMethods.First(info =>
                string.CompareOrdinal(info.Name, "IntegrationTestSample.StepImplementation.ContinueOnFailure") == 0);

            var executionResult = sandbox.ExecuteMethod(gaugeMethod);

            Assert.False(executionResult.Success);
            Assert.True(executionResult.Recoverable);
        }

        [Fact]
        public void ShouldCreateTableFromTargetType()
        {
            var sandbox = new Sandbox();
            var stepMethods = sandbox.GetStepMethods();
            var gaugeMethod = stepMethods.First(info =>
                string.CompareOrdinal(info.Name, "IntegrationTestSample.StepImplementation.ReadTable-Tabletable") == 0);

            var table = new Table(new List<string> {"foo", "bar"});
            table.AddRow(new List<string> {"foorow1", "barrow1"});
            table.AddRow(new List<string> {"foorow2", "barrow2"});

            var executionResult = sandbox.ExecuteMethod(gaugeMethod, SerializeTable(table));
            Assert.True(executionResult.Success);
        }

        [Fact]
        public void ShouldExecuteMethodAndReturnResult()
        {
            var sandbox = new Sandbox();
            var stepMethods = sandbox.GetStepMethods();
            AssertRunnerDomainDidNotLoadUsersAssembly();
            var gaugeMethod = stepMethods.First(info =>
                string.CompareOrdinal(info.Name, "IntegrationTestSample.StepImplementation.Context") == 0);

            var executionResult = sandbox.ExecuteMethod(gaugeMethod);
            Assert.True(executionResult.Success);
        }

        [Fact]
        public void ShouldGetAllStepMethods()
        {
            var sandbox = new Sandbox();
            AssertRunnerDomainDidNotLoadUsersAssembly();
            var stepMethods = sandbox.GetStepMethods();

            Assert.Equal(12, stepMethods.Count);
        }

        [Fact]
        public void ShouldGetAllStepTexts()
        {
            var sandbox = new Sandbox();
            var stepTexts = sandbox.GetAllStepTexts().ToList();

            new List<string>
            {
                "Say <what> to <who>",
                "A context step which gets executed before every scenario",
                "Step that takes a table <table>",
                "Refactoring Say <what> to <who>",
                "Refactoring A context step which gets executed before every scenario",
                "Refactoring Step that takes a table <table>"
            }.ForEach(s => Assert.Contains(s, stepTexts));
        }

        [Fact]
        public void ShouldGetPendingMessages()
        {
            var sandbox = new Sandbox();
            var stepMethods = sandbox.GetStepMethods();
            var gaugeMethod = stepMethods.First(info =>
                string.CompareOrdinal(info.Name,
                    "IntegrationTestSample.StepImplementation.SaySomething-StringwhatStringwho") == 0);

            sandbox.ExecuteMethod(gaugeMethod, "hello", "world");
            var pendingMessages = sandbox.GetAllPendingMessages().ToList();

            Assert.Contains("hello, world!", pendingMessages);
        }

        [Fact]
        public void ShouldGetStacktraceForAggregateException()
        {
            var sandbox = new Sandbox();
            var stepMethods = sandbox.GetStepMethods();
            var gaugeMethod = stepMethods.First(info =>
                string.CompareOrdinal(info.Name, "IntegrationTestSample.StepImplementation.AsyncExeption") == 0);

            var executionResult = sandbox.ExecuteMethod(gaugeMethod);

            Assert.False(executionResult.Success);
            Assert.Contains("First Exception", executionResult.StackTrace);
            Assert.Contains("Second Exception", executionResult.StackTrace);
        }

        [Fact]
        public void ShouldGetStepTextsForMethod()
        {
            var sandbox = new Sandbox();
            var stepMethods = sandbox.GetStepMethods();
            var gaugeMethod = stepMethods.First(info =>
                string.CompareOrdinal(info.Name, "IntegrationTestSample.StepImplementation.StepWithAliases") == 0);

            var stepTexts = sandbox.GetStepTexts(gaugeMethod).ToList();

            Assert.Contains("Step with text", stepTexts);
            Assert.Contains("and an alias", stepTexts);
        }

        [Fact]
        public void ShouldNotLoadTargetLibAssemblyInRunnersDomain()
        {
            new Sandbox();

            // The sample project uses a special version of Gauge Lib, versioned 0.0.0 for testing.
            // The actual Gauge CSharp runner uses a different version of Lib 
            // used by runner
            AssertRunnerDomainDidNotLoadUsersAssembly();
        }

        [Fact]
        public void SuccessIsFalseOnSerializableExceptionThrown()
        {
            const string expectedMessage = "I am a custom serializable exception";
            var sandbox = new Sandbox();
            var stepMethods = sandbox.GetStepMethods();
            var gaugeMethod = stepMethods.First(info =>
                string.CompareOrdinal(info.Name,
                    "IntegrationTestSample.StepImplementation.ThrowSerializableException") == 0);

            var executionResult = sandbox.ExecuteMethod(gaugeMethod);

            Assert.False(executionResult.Success);
            Assert.Equal(expectedMessage, executionResult.ExceptionMessage);
            Assert.Contains("IntegrationTestSample.StepImplementation.ThrowSerializableException",
                executionResult.StackTrace);
        }

        [Fact]
        public void SuccessIsFalseOnUnserializableExceptionThrown()
        {
            const string expectedMessage = "I am a custom exception";
            var sandbox = new Sandbox();
            var stepMethods = sandbox.GetStepMethods();
            AssertRunnerDomainDidNotLoadUsersAssembly();
            var gaugeMethod = stepMethods.First(info =>
                string.CompareOrdinal(info.Name,
                    "IntegrationTestSample.StepImplementation.ThrowUnserializableException") == 0);

            var executionResult = sandbox.ExecuteMethod(gaugeMethod);
            Assert.False(executionResult.Success);
            Assert.Equal(expectedMessage, executionResult.ExceptionMessage);
            Assert.Contains("IntegrationTestSample.StepImplementation.ThrowUnserializableException",
                executionResult.StackTrace);
        }
    }
}