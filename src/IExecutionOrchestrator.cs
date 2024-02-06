/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Threading.Tasks;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public interface IExecutionOrchestrator
    {
        Task<ProtoExecutionResult> ExecuteStep(GaugeMethod method, params string[] args);

        Task<ProtoExecutionResult> ExecuteHooks(string hookType, HooksStrategy strategy, IList<string> applicableTags,
            ExecutionInfo context);
        void ClearCache();

        void StartExecutionScope(string tag);
        void CloseExecutionScope();
        IEnumerable<string> GetAllPendingMessages();
        IEnumerable<string> GetAllPendingScreenshotFiles();

    }
}