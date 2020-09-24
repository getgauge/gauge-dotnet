/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gauge.Dotnet.Executor
{
    public sealed class CustomTaskScheduler : TaskScheduler, IDisposable
    {
        private BlockingCollection<Task> tasksCollection = new BlockingCollection<Task>();

        private readonly Thread thread = null;

        public CustomTaskScheduler()
        {
            thread = new Thread(new ThreadStart(Execute));
            if (!thread.IsAlive)
            {
                thread.Start();
            }
        }

        private void Execute()
        {
            foreach (var task in tasksCollection.GetConsumingEnumerable())
            {
                TryExecuteTask(task);
            }
        }
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return tasksCollection.ToArray();
        }
        protected override void QueueTask(Task task)
        {
            if (task != null)
                tasksCollection.Add(task);
        }
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        public void Dispose()
        {
            tasksCollection.CompleteAdding();
            thread.Join();
            tasksCollection.Dispose();
        }
    }
}