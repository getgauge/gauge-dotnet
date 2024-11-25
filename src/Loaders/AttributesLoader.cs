/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Xml.Linq;
using Gauge.Dotnet.Extensions;

namespace Gauge.Dotnet.Loaders;

public class AttributesLoader : IAttributesLoader
{
    private readonly IConfiguration _config;

    public AttributesLoader(IConfiguration config)
    {
        _config = config;
    }

    public virtual IEnumerable<XAttribute> GetRemovedAttributes()
    {
        var xmldoc = XDocument.Load(_config.GetGaugeCSharpProjectFile());
        var attributes = xmldoc.Descendants().Attributes("Remove");
        return attributes;
    }
}