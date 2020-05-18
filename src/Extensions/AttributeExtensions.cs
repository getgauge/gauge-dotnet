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
            return list.FirstOrDefault(argumentSyntax =>
                string.CompareOrdinal(argumentSyntax.ToFullString(), LibType.Step.FullName()) > 0);
        }
    }
}