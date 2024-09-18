/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Messages;

namespace Gauge.Dotnet.Executors;

public interface IExecutionInfoMapper
{
    dynamic ExecutionContextFrom(ExecutionInfo currentExecutionInfo);
}