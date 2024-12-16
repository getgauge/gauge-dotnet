/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Text.Json;
using Gauge.Dotnet.Exceptions;
using Microsoft.Extensions.FileProviders;

namespace Gauge.Dotnet.Loaders;

public class AssemblyLocater : IAssemblyLocater
{
    public readonly IFileProvider _fileProvider;
    public readonly ILogger<AssemblyLocater> _logger;

    public AssemblyLocater(IFileProvider fileProvider, ILogger<AssemblyLocater> logger)
    {
        _fileProvider = fileProvider;
        _logger = logger;
    }

    public string GetTestAssembly()
    {
        var depsFile = _fileProvider.GetDirectoryContents(string.Empty).FirstOrDefault(x => x.Name.EndsWith(".deps.json"))
            ?? throw new GaugeTestAssemblyNotFoundException(_fileProvider);

        return depsFile.PhysicalPath.Replace(".deps.json", ".dll");
    }

    public IEnumerable<string> GetAssembliesReferencingGaugeLib()
    {
        var depsFile = _fileProvider.GetDirectoryContents(string.Empty).FirstOrDefault(x => x.Name.EndsWith(".deps.json"))
            ?? throw new GaugeTestAssemblyNotFoundException(_fileProvider);

        try
        {
            JsonDocument jsonDoc;
            // Dispose of the stream as quickly as possible
            using (var jsonStream = depsFile.CreateReadStream())
            {
                jsonDoc = JsonDocument.Parse(jsonStream);
            }
            return jsonDoc.RootElement.GetProperty("targets")
                .EnumerateObject().SelectMany(platform => platform.Value.EnumerateObject())
                .Where(lib =>
                {
                    return lib.Value.TryGetProperty("dependencies", out JsonElement jsonElement)
                        && jsonElement.EnumerateObject().Any(dep => dep.Name == GaugeLibAssemblyName);
                })
                .Select(lib =>
                {
                    if (lib.Value.TryGetProperty("runtime", out JsonElement jsonElement))
                    {
                        return jsonElement.EnumerateObject().First().Name.Split('/').LastOrDefault();
                    }

                    return $"{lib.Name.Split('/').FirstOrDefault()}.dll";
                });
        }
        catch (Exception ex)
        {
            // If parsing the deps file failed default to returning the app assembly
            _logger.LogWarning("Unable to get list of dependencies, failed with message {message}", ex.Message);
            return [depsFile.Name.Replace(".deps.json", ".dll")];
        }
    }
}