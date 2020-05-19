/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Diagnostics;

namespace Gauge.Dotnet
{
    internal static class Program
    {
        [STAThread]
        [DebuggerHidden]
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: {0} --<start|init>", AppDomain.CurrentDomain.FriendlyName);
                Environment.Exit(1);
            }

            var phase = args[0];
            var command = GaugeCommandFactory.GetExecutor(phase);
            command.Execute();
        }
    }
}