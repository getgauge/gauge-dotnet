/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System.Text;
using Gauge.Messages;
using static Gauge.Messages.CacheFileRequest.Types;

namespace Gauge.Dotnet.Processors;

public class CacheFileProcessor : IGaugeProcessor<CacheFileRequest, Empty>
{
    private readonly IStaticLoader _loader;

    public CacheFileProcessor(IStaticLoader loader)
    {
        _loader = loader;
    }

    public async Task<Empty> Process(int stream, CacheFileRequest request)
    {
        var content = request.Content;
        var file = request.FilePath;
        var status = request.Status;
        switch (status)
        {
            case FileStatus.Changed:
            case FileStatus.Opened:
                _loader.ReloadSteps(content, file);
                break;
            case FileStatus.Created:
                if (!_loader.GetStepRegistry().IsFileCached(file))
                    await LoadFromDisk(file);
                break;
            case FileStatus.Closed:
                await LoadFromDisk(file);
                break;
            case FileStatus.Deleted:
                _loader.RemoveSteps(file);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return new Empty();
    }

    private async Task LoadFromDisk(string file)
    {
        if (!File.Exists(file)) return;
        var content = await File.ReadAllTextAsync(file, Encoding.UTF8);
        _loader.ReloadSteps(content, file);
    }
}