/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Diagnostics;
using System.Threading;
using Gauge.CSharp.Core;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class ExecutionStartingProcessor : HookExecutionProcessor
    {
        public ExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
        }

        protected override string HookType => "BeforeSuite";


        [DebuggerHidden]
        public virtual ExecutionStatusResponse Process(ExecutionStartingRequest request)
        {
            var debuggingEnv = Utils.TryReadEnvValue("DEBUGGING");
            if (debuggingEnv != null && debuggingEnv.ToLower().Equals("true"))
            {
                // if the runner is launched in DEBUG mode, let the debugger attach.
                Console.WriteLine("Runner Ready for Debugging at Process ID " +
                                  System.Diagnostics.Process.GetCurrentProcess().Id);
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
            return ExecuteHooks(request.CurrentExecutionInfo);

        }

    }
}