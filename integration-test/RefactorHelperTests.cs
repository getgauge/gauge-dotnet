﻿/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.CSharp.Lib.Attribute;
using Gauge.Dotnet.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.Dotnet.IntegrationTests;

[TestFixture]
internal class RefactorHelperTests
{
    [SetUp]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);

        File.Copy(Path.Combine(_testProjectPath, "RefactoringSample.cs"),
            Path.Combine(_testProjectPath, "RefactoringSample.copy"), true);
    }

    [TearDown]
    public void TearDown()
    {
        var sourceFileName = Path.Combine(_testProjectPath, "RefactoringSample.copy");
        File.Copy(sourceFileName, Path.Combine(_testProjectPath, "RefactoringSample.cs"), true);
        File.Delete(sourceFileName);
        Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
    }

    private readonly string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

    private void ClassicAssertStepAttributeWithTextExists(RefactoringChange result, string methodName, string text)
    {
        var name = methodName.Split('.').Last().Split('-').First();
        var tree =
            CSharpSyntaxTree.ParseText(result.FileContent);
        var root = tree.GetRoot();

        var stepTexts = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Select(
                node => new { node, attributeSyntaxes = node.AttributeLists.SelectMany(syntax => syntax.Attributes) })
            .Where(t => string.CompareOrdinal(t.node.Identifier.ValueText, name) == 0
                        &&
                        t.attributeSyntaxes.Any(
                            syntax => string.CompareOrdinal(syntax.ToFullString(), typeof(Step).ToString()) > 0))
            .SelectMany(t => t.node.AttributeLists.SelectMany(syntax => syntax.Attributes))
            .SelectMany(syntax => syntax.ArgumentList.Arguments)
            .Select(syntax => syntax.GetText().ToString().Trim('"'));
        ClassicAssert.True(stepTexts.Contains(text));
    }

    private void ClassicAssertParametersExist(RefactoringChange result, string methodName,
        IReadOnlyList<string> parameters)
    {
        var name = methodName.Split('.').Last().Split('-').First();
        var tree =
            CSharpSyntaxTree.ParseText(result.FileContent);
        var root = tree.GetRoot();
        var methodParameters = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(syntax => string.CompareOrdinal(syntax.Identifier.Text, name) == 0)
            .Select(syntax => syntax.ParameterList)
            .SelectMany(syntax => syntax.Parameters)
            .Select(syntax => syntax.Identifier.Text)
            .ToArray();

        for (var i = 0; i < parameters.Count; i++)
            ClassicAssert.AreEqual(parameters[i], methodParameters[i]);
    }

    [Test]
    public void ShouldAddParameters()
    {
        const string newStepValue = "Refactoring Say <what> to <who> in <where>";
        var gaugeMethod = new GaugeMethod
        {
            Name = "RefactoringSaySomething",
            ClassName = "RefactoringSample",
            FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs")
        };

        var parameterPositions = new[]
            {new Tuple<int, int>(0, 0), new Tuple<int, int>(1, 1), new Tuple<int, int>(-1, 2)};
        var changes = RefactorHelper.Refactor(gaugeMethod, parameterPositions,
            new List<string> { "what", "who", "where" },
            newStepValue);
        ClassicAssertStepAttributeWithTextExists(changes, gaugeMethod.Name, newStepValue);
        ClassicAssertParametersExist(changes, gaugeMethod.Name, new[] { "what", "who", "where" });
    }

    [Test]
    public void ShouldAddParametersWhenNoneExisted()
    {
        const string newStepValue = "Refactoring this is a test step <foo>";
        var gaugeMethod = new GaugeMethod
        {
            Name = "RefactoringSampleTest",
            ClassName = "RefactoringSample",
            FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs")
        };
        var parameterPositions = new[] { new Tuple<int, int>(-1, 0) };

        var changes =
            RefactorHelper.Refactor(gaugeMethod, parameterPositions, new List<string> { "foo" }, newStepValue);

        ClassicAssertStepAttributeWithTextExists(changes, gaugeMethod.Name, newStepValue);
        ClassicAssertParametersExist(changes, gaugeMethod.Name, new[] { "foo" });
    }

    [Test]
    public void ShouldAddParametersWithReservedKeywordName()
    {
        const string newStepValue = "Refactoring this is a test step <class>";

        var gaugeMethod = new GaugeMethod
        {
            Name = "RefactoringSampleTest",
            ClassName = "RefactoringSample",
            FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs")
        };
        var parameterPositions = new[] { new Tuple<int, int>(-1, 0) };

        var changes = RefactorHelper.Refactor(gaugeMethod, parameterPositions, new List<string> { "class" },
            newStepValue);

        ClassicAssertStepAttributeWithTextExists(changes, gaugeMethod.Name, newStepValue);
        ClassicAssertParametersExist(changes, gaugeMethod.Name, new[] { "@class" });
    }

    [Test]
    public void ShouldRefactorAndReturnFilesChanged()
    {
        var gaugeMethod = new GaugeMethod
        {
            Name = "RefactoringContext",
            ClassName = "RefactoringSample",
            FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs")
        };

        var expectedPath = Path.GetFullPath(Path.Combine(_testProjectPath, "RefactoringSample.cs"));

        var changes =
            RefactorHelper.Refactor(gaugeMethod, new List<Tuple<int, int>>(), new List<string>(), "foo");

        ClassicAssert.AreEqual(expectedPath, changes.FileName);
    }

    [Test]
    public void ShouldRefactorAttributeText()
    {
        var gaugeMethod = new GaugeMethod
        {
            Name = "RefactoringContext",
            ClassName = "RefactoringSample",
            FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs")
        };
        var changes = RefactorHelper.Refactor(gaugeMethod, new List<Tuple<int, int>>(), new List<string>(), "foo");

        ClassicAssertStepAttributeWithTextExists(changes, gaugeMethod.Name, "foo");
    }

    [Test]
    public void ShouldRemoveParameters()
    {
        var gaugeMethod = new GaugeMethod
        {
            Name = "RefactoringSaySomething",
            ClassName = "RefactoringSample",
            FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs")
        };
        var parameterPositions = new[] { new Tuple<int, int>(0, 0) };

        var changes = RefactorHelper.Refactor(gaugeMethod, parameterPositions, new List<string>(),
            "Refactoring Say <what> to someone");

        ClassicAssertParametersExist(changes, gaugeMethod.Name, new[] { "what" });
    }

    [Test]
    public void ShouldRemoveParametersInAnyOrder()
    {
        var gaugeMethod = new GaugeMethod
        {
            Name = "RefactoringSaySomething",
            ClassName = "RefactoringSample",
            FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs")
        };

        var parameterPositions = new[] { new Tuple<int, int>(1, 0) };

        var changes = RefactorHelper.Refactor(gaugeMethod, parameterPositions, new List<string>(),
            "Refactoring Say something to <who>");

        ClassicAssertParametersExist(changes, gaugeMethod.Name, new[] { "who" });
    }

    [Test]
    public void ShouldReorderParameters()
    {
        const string newStepValue = "Refactoring Say <who> to <what>";

        var gaugeMethod = new GaugeMethod
        {
            Name = "RefactoringSaySomething",
            ClassName = "RefactoringSample",
            FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs")
        };

        var parameterPositions = new[] { new Tuple<int, int>(0, 1), new Tuple<int, int>(1, 0) };
        var result = RefactorHelper.Refactor(gaugeMethod, parameterPositions, new List<string> { "who", "what" },
            newStepValue);

        ClassicAssertStepAttributeWithTextExists(result, gaugeMethod.Name, newStepValue);
        ClassicAssertParametersExist(result, gaugeMethod.Name, new[] { "who", "what" });
        ClassicAssert.True(result.Diffs.Any(d => d.Content == "\"Refactoring Say <who> to <what>\""));
        ClassicAssert.True(result.Diffs.Any(d => d.Content == "(string who,string what)"));
    }
}