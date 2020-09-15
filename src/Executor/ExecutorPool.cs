/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
 
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Gauge.Dotnet.Executor
{
    public class ExecutorPool : IDisposable
    {
        public bool IsMultithreading { get; internal set; }
        private ConcurrentDictionary<string, CustomTaskScheduler> _workers = new ConcurrentDictionary<string, CustomTaskScheduler>();

        public ExecutorPool(int size, bool isMultithreading)
        {
            for (int i = 1; i <= size; i++)
            {
                bool added = _workers.TryAdd(GetName(i), new CustomTaskScheduler());
                if (!added)
                {
                    Logger.Fatal("Failed to add Wroker for stream " + i);
                }
            }

            IsMultithreading = isMultithreading;
        }

        public void Dispose()
        {
            foreach (var w in _workers)
            {
                w.Value.Dispose();
            }
        }

        public Task<T> Execute<T>(int stream, Func<T> fn)
        {
            bool found = _workers.TryGetValue(GetName(stream), out CustomTaskScheduler scheduler);
            if (found)
            {
                return Task.Factory.StartNew<T>(fn, new CancellationToken(), TaskCreationOptions.None, scheduler);
            }
            throw new StreamNotFountException(stream);
        }

        private string GetName(int i)
        {
            return $"Executor-{i}";
        }

    }
}