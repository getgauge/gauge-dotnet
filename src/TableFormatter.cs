/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


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