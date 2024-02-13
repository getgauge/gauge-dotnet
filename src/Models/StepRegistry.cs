/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Gauge.Messages;
using static Gauge.Messages.StepPositionsResponse.Types;

namespace Gauge.Dotnet.Models
{
    [Serializable]
    public class StepRegistry : IStepRegistry
    {
        private Dictionary<string, List<GaugeMethod>> _registry;

        public StepRegistry()
        {
            _registry = new Dictionary<string, List<GaugeMethod>>();
        }

        public IEnumerable<string> GetStepTexts()
        {
            return _registry.Values.SelectMany(methods => methods.Select(method => method.StepText));
        }

        public void AddStep(string stepValue, GaugeMethod method)
        {
            if (!_registry.ContainsKey(stepValue)) _registry.Add(stepValue, new List<GaugeMethod>());
            _registry.GetValueOrDefault(stepValue).Add(method);
        }

        public void RemoveSteps(string filepath)
        {
            var newRegistry = new Dictionary<string, List<GaugeMethod>>();
            foreach (var (key, gaugeMethods) in _registry)
            {
                var methods = gaugeMethods.Where(method => !filepath.Equals(method.FileName)).ToList();
                if (methods.Count > 0) newRegistry[key] = methods;
            }

            _registry = newRegistry;
        }

        public IEnumerable<StepPosition> GetStepPositions(string filePath)
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


        public bool ContainsStep(string parsedStepText)
        {
            return _registry.ContainsKey(parsedStepText);
        }

        public bool HasMultipleImplementations(string parsedStepText)
        {
            return _registry[parsedStepText].Count > 1;
        }

        public GaugeMethod MethodFor(string parsedStepText)
        {
            return _registry[parsedStepText][0];
        }

        public bool HasAlias(string stepValue)
        {
            return _registry.ContainsKey(stepValue) && _registry.GetValueOrDefault(stepValue).FirstOrDefault().HasAlias;
        }

        public string GetStepText(string stepValue)
        {
            return _registry.ContainsKey(stepValue) ? _registry[stepValue][0].StepText : string.Empty;
        }

        public void Clear()
        {
            _registry = new Dictionary<string, List<GaugeMethod>>();
        }


        public IEnumerable<string> AllSteps()
        {
            return _registry.Keys;
        }

        public bool IsFileCached(string file)
        {
            foreach (var gaugeMethods in _registry.Values)
            {
                if (gaugeMethods.Any(method => file.Equals(method.FileName)))
                    return true;
            }
            return false;
        }
    }
}