using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gauge.Messages;
using static Gauge.Messages.CacheFileRequest.Types;

namespace Gauge.Dotnet.Processors
{
    internal class CacheFileRequestProcessor: IMessageProcessor
    {
        private readonly IStaticLoader _loader;

        public CacheFileRequestProcessor(IStaticLoader loader)
        {
            _loader = loader;
        }
        public Message Process(Message request)
        {
            var content = request.CacheFileRequest.Content;
            var file = request.CacheFileRequest.FilePath;
            var status = request.CacheFileRequest.Status;
            switch (status)
            {
                case FileStatus.Changed:
                case FileStatus.Opened:
                    _loader.ReloadSteps(content, file);
                    break;
                case FileStatus.Created:
                case FileStatus.Closed:
                    LoadFromDisk(file);
                    break;
                case FileStatus.Deleted:
                    _loader.RemoveSteps(file);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new Message();
        }

        private void LoadFromDisk(string file)
        {
            if (!File.Exists(file)) return;
            var content = File.ReadAllText(file, Encoding.UTF8);
            _loader.ReloadSteps(content, file);
        }
    }
}
