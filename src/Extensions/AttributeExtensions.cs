/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.Dotnet.Extensions
{
    public static class AttributeExtensions
    {
        public static AttributeListSyntax WithStepAttribute(this SyntaxList<AttributeListSyntax> list)
        {
            return list.First(syntax => GetStepAttribute(syntax.Attributes) != null);
        }

        public static AttributeSyntax GetStepAttribute(this SeparatedSyntaxList<AttributeSyntax> list)
        {
            return list.FirstOrDefault(attributeSyntax => IsStepAttribute(attributeSyntax));
        }

        /// <summary>
        /// Checks if the attribute is a Step attribute.
        /// Accepts "Step" or ends with ".Step" (handles qualified names) or the full type name.
        /// </summary>
        /// <param name="attributeSyntax">The attribute to check.</param>
        /// <returns>True if the attribute is a Step attribute, false otherwise.</returns>
        public static bool IsStepAttribute(AttributeSyntax attributeSyntax)
        {
            if (attributeSyntax?.Name == null)
                return false;

            var nameString = attributeSyntax.Name.ToString();
            return nameString == "Step"
                || nameString.EndsWith(".Step", System.StringComparison.Ordinal)
                || nameString == LibType.Step.FullName()
                || nameString.EndsWith("." + LibType.Step.FullName(), System.StringComparison.Ordinal);
        }
    }
}