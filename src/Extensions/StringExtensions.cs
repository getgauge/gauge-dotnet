/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Gauge.Dotnet.Extensions
{
    public static class StringExtensions
    {
        public static string ToValidCSharpIdentifier(this string str, bool camelCase = true)
        {
            str = str.Trim();

            if (!IsCSharpKeyword(str) && SyntaxFacts.IsValidIdentifier(str))
                return str;

            str = camelCase
                ? str.Split(' ').Select(s => s.Capitalize()).Aggregate(string.Concat)
                : str.Replace(" ", "");
            var result = new StringBuilder();

            if (!SyntaxFacts.IsIdentifierStartCharacter(str[0]))
                result.Append('_');

            foreach (var c in str.Where(SyntaxFacts.IsIdentifierPartCharacter))
                result.Append(c);

            var retval = result.ToString();

            if (IsCSharpKeyword(retval))
                retval = string.Concat('@', retval);

            return retval;
        }

        private static bool IsCSharpKeyword(string retval)
        {
            return SyntaxFacts.GetKeywordKind(retval) != SyntaxKind.None;
        }

        private static string Capitalize(this string s)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }
    }
}