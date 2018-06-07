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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.Dotnet
{
    public sealed class StaticLoader
    {

        private static readonly StaticLoader instance = new StaticLoader();

        static StaticLoader()
        {
        }

        public StaticLoader()
        {
        }

        public static StaticLoader Instance
        {
            get
            {
                return instance;
            }
        }

        public void LoadImplementations()
        {
            var classFiles = Directory.EnumerateFiles(Environment.GetEnvironmentVariable("GAUGE_PROJECT_ROOT"), "*.cs", SearchOption.AllDirectories);
            foreach (var f in classFiles)
            {
                var steps = GetStesFrom(f);
                AddStepsToRegsitry(f,steps);
            }
        }

        private void AddStepsToRegsitry(string fileName, IEnumerable<MethodDeclarationSyntax> stepMethods)
        {
            foreach (var stepMethod in stepMethods)
            {
                var attributeListSyntax = stepMethod.AttributeLists.WithStepAttribute();
                var attributeSyntax = attributeListSyntax.Attributes.GetStepAttribute();
                var stepText = attributeSyntax.ArgumentList.Arguments.FirstOrDefault().ToString().Trim('"');
                var stepValue = stepText.GetStepValue();
                var classDef = stepMethod.Parent as ClassDeclarationSyntax;
                var entry = new GaugeMethod()
                {
                    Name = stepMethod.Identifier.ToString(),
                    ParameterCount = stepMethod.ParameterList.Parameters.Count,
                    ContinueOnFailure = stepMethod.IsRecoverable(),
                    StepText = stepText,
                    StepValue = stepValue,
                    Span = stepMethod.GetLocation().GetLineSpan(),
                    ClassName = classDef.Identifier.ValueText,
                    FileName = fileName,
                };
                StepRegistry.Instance.AddStep(stepValue, entry);
            }
        }

        private static IEnumerable<MethodDeclarationSyntax> GetStesFrom(string f)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(f));
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