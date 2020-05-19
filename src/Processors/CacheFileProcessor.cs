/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using Gauge.Messages;
using static Gauge.Messages.CacheFileRequest.Types;

namespace Gauge.Dotnet.Processors
{
    public class CacheFileProcessor
    {
        private readonly IStaticLoader _loader;

        public CacheFileProcessor(IStaticLoader loader)
        {
            _loader = loader;
        }

        public Empty Process(CacheFileRequest request)
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
                        LoadFromDisk(file);
                    break;
                case FileStatus.Closed:
                    LoadFromDisk(file);
                    break;
                case FileStatus.Deleted:
                    _loader.RemoveSteps(file);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Empty();
        }

        private void LoadFromDisk(string file)
        {
            if (!File.Exists(file)) return;
            var content = File.ReadAllText(file, Encoding.UTF8);
            _loader.ReloadSteps(content, file);
        }
    }
}