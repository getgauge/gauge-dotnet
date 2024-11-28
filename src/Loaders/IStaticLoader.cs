/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Loaders
{
    public interface IStaticLoader
    {
        IStepRegistry GetStepRegistry();
        void ReloadSteps(string content, string file);
        void RemoveSteps(string file);
        void LoadStepsFromText(string content, string file);

        void LoadImplementations();
    }
}