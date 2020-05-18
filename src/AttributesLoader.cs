/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Xml.Linq;
using Gauge.CSharp.Core;

namespace Gauge.Dotnet
{
    public class AttributesLoader : IAttributesLoader
    {
        public virtual IEnumerable<XAttribute> GetRemovedAttributes()
        {
            var xmldoc = XDocument.Load(Utils.ReadEnvValue("GAUGE_CSHARP_PROJECT_FILE"));
            var attributes = xmldoc.Descendants().Attributes("Remove");
            return attributes;
        }
    }
}