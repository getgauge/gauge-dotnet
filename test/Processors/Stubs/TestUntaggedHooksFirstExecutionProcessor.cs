/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet.UnitTests.Processors.Stubs
{
    public class TestUntaggedHooksFirstExecutionProcessor : UntaggedHooksFirstExecutionProcessor
    {
        public TestUntaggedHooksFirstExecutionProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
        }

        protected override string HookType => throw new NotImplementedException();

        public HooksStrategy GetHooksStrategy()
        {
            return Strategy;
        }
    }
}