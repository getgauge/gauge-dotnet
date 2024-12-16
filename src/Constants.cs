namespace Gauge.Dotnet;

public static class Constants
{
    public static string GaugeLibAssemblyName => "Gauge.CSharp.Lib";
    public static string CSharpLibVersion => "0.12.0";

    public enum DataStoreType
    {
        Suite = 0,
        Spec,
        Scenario
    }

    public enum LibType
    {
        Table,
        Step,
        BeforeSuite,
        BeforeSpec,
        BeforeScenario,
        BeforeStep,
        AfterStep,
        BeforeConcept,
        AfterConcept,
        AfterScenario,
        AfterSpec,
        AfterSuite,
        FilteredHookAttribute,
        TagAggregationBehaviourAttribute,
        DataStore,
        MessageCollector,
        ScreenshotFilesCollector,
        ContinueOnFailure,
        DefaultScreenshotWriter,
        DefaultClassInstanceManager,
        GaugeScreenshots,
        ExecutionContext,
    }

    private static readonly Dictionary<LibType, string> typeNames = new Dictionary<LibType, string>
    {
        {LibType.Table, "Gauge.CSharp.Lib.Table"},
        {LibType.Step, "Gauge.CSharp.Lib.Attribute.Step"},
        {LibType.BeforeSuite, "Gauge.CSharp.Lib.Attribute.BeforeSuite"},
        {LibType.BeforeSpec, "Gauge.CSharp.Lib.Attribute.BeforeSpec"},
        {LibType.BeforeScenario, "Gauge.CSharp.Lib.Attribute.BeforeScenario"},
        {LibType.BeforeStep, "Gauge.CSharp.Lib.Attribute.BeforeStep"},
        {LibType.AfterStep, "Gauge.CSharp.Lib.Attribute.AfterStep"},
        {LibType.BeforeConcept, "Gauge.CSharp.Lib.Attribute.BeforeConcept"},
        {LibType.AfterConcept, "Gauge.CSharp.Lib.Attribute.AfterConcept"},
        {LibType.AfterScenario, "Gauge.CSharp.Lib.Attribute.AfterScenario"},
        {LibType.AfterSpec, "Gauge.CSharp.Lib.Attribute.AfterSpec"},
        {LibType.AfterSuite, "Gauge.CSharp.Lib.Attribute.AfterSuite"},
        {LibType.FilteredHookAttribute, "Gauge.CSharp.Lib.Attribute.FilteredHookAttribute"},
        {LibType.TagAggregationBehaviourAttribute, "Gauge.CSharp.Lib.Attribute.TagAggregationBehaviourAttribute"},
        {LibType.DataStore, "Gauge.CSharp.Lib.DataStore"},
        {LibType.MessageCollector, "Gauge.CSharp.Lib.MessageCollector"},
        {LibType.ScreenshotFilesCollector, "Gauge.CSharp.Lib.ScreenshotFilesCollector"},
        {LibType.ContinueOnFailure, "Gauge.CSharp.Lib.Attribute.ContinueOnFailure"},
        {LibType.DefaultScreenshotWriter, "Gauge.CSharp.Lib.DefaultScreenshotWriter"},
        {LibType.DefaultClassInstanceManager, "Gauge.CSharp.Lib.DefaultClassInstanceManager"},
        {LibType.GaugeScreenshots, "Gauge.CSharp.Lib.GaugeScreenshots"},
        {LibType.ExecutionContext, "Gauge.CSharp.Lib.ExecutionContext"},
    };

    public static string FullName(this LibType type)
    {
        return typeNames[type];
    }
}
