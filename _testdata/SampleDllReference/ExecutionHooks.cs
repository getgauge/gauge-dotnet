/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.CSharp.Lib.Attribute;

namespace IntegrationTestSample
{
    internal class ExecutionHooks
    {
        [BeforeSuite]
        public void BeforeSuite()
        {
        }

        [AfterSuite]
        public void AfterSuite()
        {
        }

        [BeforeStep]
        public void BeforeStep()
        {
        }

        [AfterStep]
        public void AfterStep()
        {
        }

        [BeforeSpec]
        public void BeforeSpec()
        {
        }

        [AfterSpec]
        public void AfterSpec()
        {
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
        }

        [AfterScenario]
        public void AfterScenario()
        {
        }
    }
}