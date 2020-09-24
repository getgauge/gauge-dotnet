/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Wrappers;

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
                    return new StartCommand(() =>
                        {
                            var loader = new StaticLoader(new AttributesLoader(), new DirectoryWrapper());
                            loader.LoadImplementations();
                            return new GaugeListener(loader);
                        },
                        () => new GaugeProjectBuilder());
            }
        }
    }
}