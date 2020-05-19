/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Strategy;

namespace Gauge.Dotnet.Processors
{
    public abstract class UntaggedHooksFirstExecutionProcessor : HookExecutionProcessor
    {
        protected UntaggedHooksFirstExecutionProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
            Strategy = new UntaggedHooksFirstStrategy();
        }
    }
}