/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Processors;
using Gauge.Messages;

namespace Gauge.Dotnet.UnitTests.Processors
{
    [TestFixture]
    public class CacheFileProcessorTests
    {
        private const string content = "using Gauge.CSharp.Lib.Attributes;\n" +
                              "namespace foobar\n" +
                              "{\n" +
                              "    public class FooBar\n" +
                              "    {\n" +
                              "        [Step(\"goodbye\",\"adieu\", \"sayonara\")]\n" +
                              "        public void farewell()\n" +
                              "        {\n" +
                              "        }\n" +
                              "    }\n" +
                              "}\n";
        private const string fileName = "foo.cs";

        [Test]
        public async Task ShouldProcessMessage()
        {
            var mockLoader = new Mock<IStaticLoader>();
            mockLoader.Setup(x => x.LoadImplementations());
            mockLoader.Setup(x => x.ReloadSteps(content, fileName)).Verifiable();
            var processor = new CacheFileProcessor(mockLoader.Object);
            var request = new CacheFileRequest
            {
                Content = content,
                FilePath = fileName,
                Status = CacheFileRequest.Types.FileStatus.Opened
            };

            await processor.Process(1, request);

            mockLoader.Verify();
            mockLoader.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldProcessRequestWithDeleteStatus()
        {
            var mockLoader = new Mock<IStaticLoader>();
            mockLoader.Setup(x => x.LoadImplementations());
            mockLoader.Setup(x => x.RemoveSteps(fileName)).Verifiable();
            var processor = new CacheFileProcessor(mockLoader.Object);
            var request = new CacheFileRequest
            {
                FilePath = fileName,
                Status = CacheFileRequest.Types.FileStatus.Deleted
            };

            await processor.Process(1, request);

            mockLoader.Verify();
            mockLoader.VerifyNoOtherCalls();
        }
    }
}