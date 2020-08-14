/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.CSharp.Lib;
using NUnit.Framework;

namespace Gauge.Dotnet.IntegrationTests
{
    public class IntegrationTestsBase
    {
        protected string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);
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

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }
    }
}