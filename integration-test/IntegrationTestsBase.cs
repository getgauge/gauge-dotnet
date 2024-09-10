/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.CSharp.Lib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gauge.Dotnet.IntegrationTests;

public class IntegrationTestsBase
{
    protected readonly ILoggerFactory _loggerFactory = new LoggerFactory();
    protected IConfiguration _configuration;
    protected string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

    [SetUp]
    public void Setup()
    {
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(new Dictionary<string, string> { { "GAUGE_PROJECT_ROOT", _testProjectPath } });
        _configuration = builder.Build();
    }

    public static string SerializeTable(Table table)
    {
        var serializer = new DataContractJsonSerializer(typeof(Table));
        using (var memoryStream = new MemoryStream())
        {
            serializer.WriteObject(memoryStream, table);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}