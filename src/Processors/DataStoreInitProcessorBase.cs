// Copyright 2015 ThoughtWorks, Inc.
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
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public abstract class DataStoreInitProcessorBase : IMessageProcessor
    {
        private readonly Type _dataStoreFactoryType;
        private DataStoreType _dataStoreType;

        protected DataStoreInitProcessorBase(IAssemblyLoader assemblyLoader, DataStoreType scenario)
        {
            _dataStoreType = scenario;
            _dataStoreFactoryType = assemblyLoader.GetLibType(LibType.DataStoreFactory);
        }

        public Message Process(Message request)
        {
            var initMethod = _dataStoreFactoryType.GetMethod($"Initialize{_dataStoreType}DataStore");
            initMethod.Invoke(null, null);
            return new DefaultProcessor().Process(request);
        }
    }
}