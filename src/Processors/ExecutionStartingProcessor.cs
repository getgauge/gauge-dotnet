/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Diagnostics;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Extensions;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ExecutionStartingProcessor : HookExecutionProcessor, IGaugeProcessor<ExecutionStartingRequest, ExecutionStatusResponse>
{
    public ExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
    }

    protected override string HookType => "BeforeSuite";


    [DebuggerHidden]
    public virtual async Task<ExecutionStatusResponse> Process(int streamId, ExecutionStartingRequest request)
    {
        if (Configuration.IsDebugging())
        {
            // if the runner is launched in DEBUG mode, let the debugger attach.
            Console.WriteLine("Runner Ready for Debugging at Process ID " + Environment.ProcessId);
            var j = 0;
            while (!Debugger.IsAttached)
            {
                j++;
                //Trying to debug, wait for a debugger to attach
                Thread.Sleep(100);
                //Timeout, no debugger connected, break out into a normal execution.
                if (j == 300)
                    break;
            }
        }
        return await ExecuteHooks(streamId, request.CurrentExecutionInfo);
    }

}