/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.Dotnet
{
    public static class RefactorHelper
    {
        public static RefactoringChange Refactor(GaugeMethod method, IList<Tuple<int, int>> parameterPositions,
            IList<string> parameters, string newStepValue)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(method.FileName));
            var root = tree.GetRoot();
            var stepMethods = from node in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                let attributeSyntaxes = node.AttributeLists.SelectMany(syntax => syntax.Attributes)
                let classDef = node.Parent as ClassDeclarationSyntax
                where string.CompareOrdinal(node.Identifier.ValueText, method.Name) == 0
                      && string.CompareOrdinal(classDef.Identifier.ValueText, method.ClassName) == 0
                      && attributeSyntaxes.Any(syntax =>
                          string.CompareOrdinal(syntax.ToFullString(), LibType.Step.FullName()) > 0)
                select node;

            var stepMethod = stepMethods.First();
            var stepSpan = stepMethod.AttributeLists.WithStepAttribute().Attributes.GetStepAttribute()
                .ArgumentList.Arguments.First().GetLocation().GetLineSpan();
            var paramsSpan = stepMethod.ParameterList.GetLocation().GetLineSpan();
            var updatedAttribute = ReplaceAttribute(stepMethod, newStepValue);
            var updatedParameters = ReplaceParameters(stepMethod, parameterPositions, parameters);
            var declarationSyntax = stepMethod
                .WithAttributeLists(updatedAttribute)
                .WithParameterList(updatedParameters);
            var replaceNode = root.ReplaceNode(stepMethod, declarationSyntax);
            var change = new RefactoringChange
            {
                FileName = method.FileName,
                Diffs = new List<Diff>(),
                FileContent = replaceNode.ToFullString()
            };
            change.Diffs.Add(CreateDiff(stepSpan, $"\"{newStepValue}\""));
            change.Diffs.Add(CreateDiff(paramsSpan, updatedParameters.ToFullString().Trim()));
            return change;
        }

        private static Diff CreateDiff(FileLinePositionSpan fileLinePositionSpan, string text)
        {
            var start = new Position(fileLinePositionSpan.StartLinePosition.Line + 1,
                fileLinePositionSpan.StartLinePosition.Character);
            var end = new Position(fileLinePositionSpan.EndLinePosition.Line + 1,
                fileLinePositionSpan.EndLinePosition.Character);
            return new Diff(text, new Gauge.Dotnet.Models.Range(start, end));
        }

        private static ParameterListSyntax ReplaceParameters(MethodDeclarationSyntax methodDeclarationSyntax,
            IEnumerable<Tuple<int, int>> parameterPositions, IList<string> parameters)
        {
            var parameterListSyntax = methodDeclarationSyntax.ParameterList;
            var newParams = new SeparatedSyntaxList<ParameterSyntax>();
            newParams = parameterPositions.OrderBy(position => position.Item2)
                .Aggregate(newParams, (current, parameterPosition) =>
                    current.Add(parameterPosition.Item1 == -1
                        ? CreateParameter(parameters[parameterPosition.Item2])
                        : parameterListSyntax.Parameters[parameterPosition.Item1]));
            return parameterListSyntax.WithParameters(newParams);
        }

        private static ParameterSyntax CreateParameter(string text)
        {
            // Could not get SyntaxFactory.Parameter to work properly, so ended up parsing code as string
            return SyntaxFactory.ParseParameterList(string.Format("string {0}", text.ToValidCSharpIdentifier(false)))
                .Parameters[0];
        }

        private static SyntaxList<AttributeListSyntax> ReplaceAttribute(MethodDeclarationSyntax methodDeclarationSyntax,
            string newStepText)
        {
            var attributeListSyntax = methodDeclarationSyntax.AttributeLists.WithStepAttribute();
            var attributeSyntax = attributeListSyntax.Attributes.GetStepAttribute();
            var attributeArgumentSyntax = attributeSyntax.ArgumentList.Arguments.FirstOrDefault();

            if (attributeArgumentSyntax == null)
                return default(SyntaxList<AttributeListSyntax>);
            var newAttributeArgumentSyntax = attributeArgumentSyntax.WithExpression(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.ParseToken(string.Format("\"{0}\"", newStepText))));

            var attributeArgumentListSyntax =
                attributeSyntax.ArgumentList.WithArguments(
                    new SeparatedSyntaxList<AttributeArgumentSyntax>().Add(newAttributeArgumentSyntax));
            var newAttributeSyntax = attributeSyntax.WithArgumentList(attributeArgumentListSyntax);

            var newAttributes = attributeListSyntax.Attributes.Remove(attributeSyntax).Add(newAttributeSyntax);
            var newAttributeListSyntax = attributeListSyntax.WithAttributes(newAttributes);

            return methodDeclarationSyntax.AttributeLists.Remove(attributeListSyntax).Add(newAttributeListSyntax);
        }
    }
}