/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Text.Json;
using Gauge.Dotnet.Exceptions;
using Microsoft.Extensions.FileProviders;

namespace Gauge.Dotnet.Loaders;

public static class AssemblyLocater
{
    public static string GetTestAssembly(IFileProvider fileProvider)
    {
        var depsFile = fileProvider.GetDirectoryContents(string.Empty).FirstOrDefault(x => x.Name.EndsWith(".deps.json"))
            ?? throw new GaugeTestAssemblyNotFoundException(fileProvider);

        return depsFile.PhysicalPath.Replace(".deps.json", ".dll");
    }

    public static IEnumerable<string> GetAssembliesReferencingGaugeLib(IFileProvider fileProvider, ILogger logger)
    {
        var depsFile = fileProvider.GetDirectoryContents(string.Empty).FirstOrDefault(x => x.Name.EndsWith(".deps.json"))
            ?? throw new GaugeTestAssemblyNotFoundException(fileProvider);

        try
        {
            var jsonDoc = JsonDocument.Parse(depsFile.CreateReadStream());
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
            logger.LogWarning("Unable to get list of dependencies, failed with message {message}", ex.Message);
            return [depsFile.Name.Replace(".deps.json", ".dll")];
        }
    }
}