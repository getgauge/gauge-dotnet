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

using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class TableFormatter : ITableFormatter
    {
        private readonly IActivatorWrapper _activatorWrapper;
        private readonly IAssemblyLoader _assemblyLoader;

        public TableFormatter(IAssemblyLoader assemblyLoader, IActivatorWrapper activatorWrapper)
        {
            _assemblyLoader = assemblyLoader;
            _activatorWrapper = activatorWrapper;
        }

        public string GetJSON(ProtoTable table)
        {
            var tableType = _assemblyLoader.GetLibType(LibType.Table);
            dynamic table1 = _activatorWrapper.CreateInstance(tableType, table.Headers.Cells.ToList());
            foreach (var protoTableRow in table.Rows)
                table1.AddRow(protoTableRow.Cells.ToList());
            var serializer = new DataContractJsonSerializer(tableType);
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, table1);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}