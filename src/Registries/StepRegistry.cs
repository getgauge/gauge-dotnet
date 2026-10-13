/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Models;
using Gauge.Messages;
using static Gauge.Messages.StepPositionsResponse.Types;

namespace Gauge.Dotnet.Registries;

[Serializable]
public class StepRegistry : IStepRegistry
{
    private readonly object _lock = new();
    private Dictionary<string, List<GaugeMethod>> _registry;

    public StepRegistry()
    {
        _registry = new Dictionary<string, List<GaugeMethod>>();
    }

    public int Count { get { lock (_lock) { return _registry.Count; } } }

    public IEnumerable<string> GetStepTexts()
    {
        lock (_lock)
        {
            return _registry.Values.SelectMany(methods => methods.Select(method => method.StepText)).ToList();
        }
    }

    public void AddStep(string stepValue, GaugeMethod method)
    {
        lock (_lock)
        {
            if (!_registry.ContainsKey(stepValue)) _registry.Add(stepValue, new List<GaugeMethod>());
            _registry.GetValueOrDefault(stepValue).Add(method);
        }
    }

    public void RemoveSteps(string filepath)
    {
        lock (_lock)
        {
            RemoveStepsInternal(filepath);
        }
    }

    public void ReplaceSteps(string filepath, IReadOnlyList<(string stepValue, GaugeMethod method)> newSteps)
    {
        lock (_lock)
        {
            RemoveStepsInternal(filepath);
            foreach (var (stepValue, method) in newSteps)
                AddStepInternal(stepValue, method);
        }
    }

    public StepLookupResult LookupStep(string parsedStepText)
    {
        lock (_lock)
        {
            if (!_registry.TryGetValue(parsedStepText, out var methods))
                return new StepLookupResult(false, false, Array.Empty<GaugeMethod>());
            return new StepLookupResult(true, methods.Count > 1, methods.ToList());
        }
    }

    public IEnumerable<StepPosition> GetStepPositions(string filePath)
    {
        lock (_lock)
        {
            var positions = new List<StepPosition>();
            foreach (var (stepValue, gaugeMethods) in _registry)
            {
                foreach (var m in gaugeMethods)
                {
                    if (!m.IsExternal && m.FileName.Equals(filePath))
                    {
                        var p = new StepPosition
                        {
                            StepValue = stepValue,
                            Span = new Span
                            {
                                Start = m.Span.StartLinePosition.Line + 1,
                                StartChar = m.Span.StartLinePosition.Character,
                                End = m.Span.EndLinePosition.Line + 1,
                                EndChar = m.Span.EndLinePosition.Character
                            }
                        };
                        positions.Add(p);
                    }
                }
            }
            return positions;
        }
    }

    public bool HasAlias(string stepValue)
    {
        lock (_lock)
        {
            return _registry.ContainsKey(stepValue) && _registry.GetValueOrDefault(stepValue).FirstOrDefault().HasAlias;
        }
    }

    public string GetStepText(string stepValue)
    {
        lock (_lock)
        {
            return _registry.ContainsKey(stepValue) ? _registry[stepValue][0].StepText : string.Empty;
        }
    }

    public void Clear()
    {
        lock (_lock) { _registry = new Dictionary<string, List<GaugeMethod>>(); }
    }

    public IEnumerable<string> AllSteps()
    {
        lock (_lock) { return _registry.Keys.ToList(); }
    }

    public bool IsFileCached(string file)
    {
        lock (_lock)
        {
            foreach (var gaugeMethods in _registry.Values)
            {
                if (gaugeMethods.Any(method => file.Equals(method.FileName)))
                    return true;
            }
            return false;
        }
    }

    private void RemoveStepsInternal(string filepath)
    {
        var newRegistry = new Dictionary<string, List<GaugeMethod>>();
        foreach (var (key, gaugeMethods) in _registry)
        {
            var methods = gaugeMethods.Where(method => !filepath.Equals(method.FileName)).ToList();
            if (methods.Count > 0) newRegistry[key] = methods;
        }
        _registry = newRegistry;
    }

    private void AddStepInternal(string stepValue, GaugeMethod method)
    {
        if (!_registry.ContainsKey(stepValue))
            _registry.Add(stepValue, new List<GaugeMethod>());
        _registry.GetValueOrDefault(stepValue).Add(method);
    }
}