/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Helpers;
using Gauge.Dotnet.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.Dotnet
{
    public sealed class StaticLoader : IStaticLoader
    {
        private readonly IAttributesLoader _attributesLoader;
        private readonly IStepRegistry _stepRegistry;


        public StaticLoader(IAttributesLoader attributesLoader)
        {
            _stepRegistry = new StepRegistry();
            _attributesLoader = attributesLoader;
        }

        public IStepRegistry GetStepRegistry()
        {
            return _stepRegistry;
        }


        public void LoadStepsFromText(string content, string filepath)
        {
            var steps = GetStepsFrom(content);
            AddStepsToRegistry(filepath, steps);
        }

        public void ReloadSteps(string content, string filepath)
        {
            if (IsFileRemoved(filepath)) return;
            _stepRegistry.RemoveSteps(filepath);
            LoadStepsFromText(content, filepath);
        }

        public void RemoveSteps(string file)
        {
            _stepRegistry.RemoveSteps(file);
        }

        private bool IsFileRemoved(string file)
        {
            var attributes = _attributesLoader.GetRemovedAttributes();
            var removedFiles = FileHelper.GetRemovedDirFiles();

            var isFileRemoved =
                attributes.Any(attribute => Path.Combine(Utils.GaugeProjectRoot, attribute.Value) == file) ||
                removedFiles.Contains(file);
            return isFileRemoved;
        }

        internal void LoadImplementations()
        {
            var classFiles = Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.cs",
                SearchOption.AllDirectories).ToList();
            var attributes = _attributesLoader.GetRemovedAttributes();
            foreach (var attribute in attributes)
                classFiles.Remove(Path.Combine(Utils.GaugeProjectRoot, attribute.Value));
            var removedFiles = FileHelper.GetRemovedDirFiles();
            var wantedFiles = classFiles.Except(removedFiles);
            foreach (var f in wantedFiles) LoadStepsFromText(File.ReadAllText(f), f);
        }

        private void AddStepsToRegistry(string fileName, IEnumerable<MethodDeclarationSyntax> stepMethods)
        {
            foreach (var stepMethod in stepMethods)
            {
                var attributeListSyntax = stepMethod.AttributeLists.WithStepAttribute();
                var attributeSyntax = attributeListSyntax.Attributes.GetStepAttribute();
                var stepTextsSyntax = attributeSyntax.ArgumentList.Arguments.ToList();
                var stepTexts = stepTextsSyntax.Select(s => s.ToString().Trim('"'));
                var hasAlias = stepTexts.Count() > 1;
                foreach (var stepText in stepTexts)
                {
                    var stepValue = Regex.Replace(stepText, @"(<.*?>)", @"{}");
                    var classDef = stepMethod.Parent as ClassDeclarationSyntax;
                    var entry = new GaugeMethod
                    {
                        Name = stepMethod.Identifier.ValueText,
                        ParameterCount = stepMethod.ParameterList.Parameters.Count,
                        StepText = stepText,
                        HasAlias = hasAlias,
                        Aliases = stepTexts,
                        StepValue = stepValue,
                        Span = stepMethod.GetLocation().GetLineSpan(),
                        ClassName = classDef.Identifier.ValueText,
                        FileName = fileName,
                        IsExternal = false
                    };
                    _stepRegistry.AddStep(stepValue, entry);
                }
            }
        }

        private static IEnumerable<MethodDeclarationSyntax> GetStepsFrom(string content)
        {
            var tree = CSharpSyntaxTree.ParseText(content);
            var root = tree.GetRoot();

            var stepMethods = from node in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                let attributeSyntaxes = node.AttributeLists.SelectMany(syntax => syntax.Attributes)
                where attributeSyntaxes.Any(syntax =>
                    string.CompareOrdinal(syntax.ToFullString(), LibType.Step.FullName()) > 0)
                select node;
            return stepMethods;
        }
    }
}