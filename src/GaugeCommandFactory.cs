/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

namespace Gauge.Dotnet
{
    public class GaugeCommandFactory
    {
        public static IGaugeCommand GetExecutor(string phase)
        {
            switch (phase)
            {
                case "--init":
                    return new SetupCommand();
                default:
                    return new StartCommand(new GaugeProjectBuilder(), typeof(GaugeListener));
            }
        }
    }
}