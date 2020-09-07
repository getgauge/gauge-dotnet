/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public interface IExecutionOrchestrator
    {
        ProtoExecutionResult ExecuteStep(GaugeMethod method, params string[] args);

        ProtoExecutionResult ExecuteHooks(string hookType, HooksStrategy strategy, IList<string> applicableTags,
            ExecutionInfo context);
        void ClearCache();

        void StartExecutionScope(string tag);
        void CloseExecutionScope();
        IEnumerable<string> GetAllPendingMessages();
        IEnumerable<string> GetAllPendingScreenshotFiles();

    }
}