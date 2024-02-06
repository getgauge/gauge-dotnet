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
        private ConcurrentDictionary<string, TaskFactory> _workers = new ConcurrentDictionary<string, TaskFactory>();
        
        public ExecutorPool(int size, bool isMultithreading)
        {
            for (int i = 1; i <= size; i++)
            {
                bool added = _workers.TryAdd(GetName(i), new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, new CustomTaskScheduler()));
                if (!added)
                {
                    Logger.Fatal("Failed to add Worker for stream " + i);
                }
            }

            IsMultithreading = isMultithreading;
        }

        public void Dispose()
        {
            foreach (var w in _workers)
            {
                ((CustomTaskScheduler)w.Value.Scheduler)?.Dispose();
            }
        }

        public Task<T> Execute<T>(int stream, Func<T> fn)
        {
            bool found = _workers.TryGetValue(GetName(stream), out TaskFactory taskFactory);
            if (found)
            {
                return taskFactory.StartNew(fn);
            }
            throw new StreamNotFountException(stream);
        }
        public Task<T> Execute<T>(int stream, Func<Task<T>> fn)
        {
            bool found = _workers.TryGetValue(GetName(stream), out TaskFactory taskFactory);
            if (found)
            {
                return taskFactory.StartNew(fn).Unwrap();
            }
            throw new StreamNotFountException(stream);
        }

        private string GetName(int i)
        {
            return $"Executor-{i}";
        }

    }
}