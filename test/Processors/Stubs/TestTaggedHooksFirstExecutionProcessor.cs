/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Microsoft.Extensions.Configuration;

namespace Gauge.Dotnet.UnitTests.Processors.Stubs;

public class TestTaggedHooksFirstExecutionProcessor : TaggedHooksFirstExecutionProcessor
{
    public TestTaggedHooksFirstExecutionProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
    }

    protected override string HookType => throw new NotImplementedException();

    public HooksStrategy GetHooksStrategy()
    {
        return Strategy;
    }
}