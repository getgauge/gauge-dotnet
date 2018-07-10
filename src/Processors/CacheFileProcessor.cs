// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Text;
using Gauge.Messages;
using static Gauge.Messages.CacheFileRequest.Types;

namespace Gauge.Dotnet.Processors
{
    public class CacheFileProcessor : IMessageProcessor
    {
        private readonly IStaticLoader _loader;

        public CacheFileProcessor(IStaticLoader loader)
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