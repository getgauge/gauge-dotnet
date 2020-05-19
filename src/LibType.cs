/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;

namespace Gauge.Dotnet
{
    public enum LibType
    {
        Table,
        Step,
        BeforeSuite,
        BeforeSpec,
        BeforeScenario,
        BeforeStep,
        AfterStep,
        AfterScenario,
        AfterSpec,
        AfterSuite,
        FilteredHookAttribute,
        TagAggregationBehaviourAttribute,
        DataStoreFactory,
        MessageCollector,
        ScreenshotFilesCollector,
        ContinueOnFailure,
        DefaultScreenshotWriter,
        DefaultClassInstanceManager,
        GaugeScreenshots
    }

    public static class LibTypeExtensions
    {
        private static readonly Dictionary<LibType, string> typeNames = new Dictionary<LibType, string>
        {
            {LibType.Table, "Gauge.CSharp.Lib.Table"},
            {LibType.Step, "Gauge.CSharp.Lib.Attribute.Step"},
            {LibType.BeforeSuite, "Gauge.CSharp.Lib.Attribute.BeforeSuite"},
            {LibType.BeforeSpec, "Gauge.CSharp.Lib.Attribute.BeforeSpec"},
            {LibType.BeforeScenario, "Gauge.CSharp.Lib.Attribute.BeforeScenario"},
            {LibType.BeforeStep, "Gauge.CSharp.Lib.Attribute.BeforeStep"},
            {LibType.AfterStep, "Gauge.CSharp.Lib.Attribute.AfterStep"},
            {LibType.AfterScenario, "Gauge.CSharp.Lib.Attribute.AfterScenario"},
            {LibType.AfterSpec, "Gauge.CSharp.Lib.Attribute.AfterSpec"},
            {LibType.AfterSuite, "Gauge.CSharp.Lib.Attribute.AfterSuite"},
            {LibType.FilteredHookAttribute, "Gauge.CSharp.Lib.Attribute.FilteredHookAttribute"},
            {LibType.TagAggregationBehaviourAttribute, "Gauge.CSharp.Lib.Attribute.TagAggregationBehaviourAttribute"},
            {LibType.DataStoreFactory, "Gauge.CSharp.Lib.DataStoreFactory"},
            {LibType.MessageCollector, "Gauge.CSharp.Lib.MessageCollector"},
            {LibType.ScreenshotFilesCollector, "Gauge.CSharp.Lib.ScreenshotFilesCollector"},
            {LibType.ContinueOnFailure, "Gauge.CSharp.Lib.Attribute.ContinueOnFailure"},
            {LibType.DefaultScreenshotWriter, "Gauge.CSharp.Lib.DefaultScreenshotWriter"},
            {LibType.DefaultClassInstanceManager, "Gauge.CSharp.Lib.DefaultClassInstanceManager"},
            {LibType.GaugeScreenshots, "Gauge.CSharp.Lib.GaugeScreenshots"},
        };

        public static string FullName(this LibType type)
        {
            return typeNames[type];
        }
    }
}