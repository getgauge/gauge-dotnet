/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


namespace Gauge.Dotnet
{
    public class AssemblyPath
    {
        private readonly string _path;

        public AssemblyPath(string path)
        {
            this._path = path;
        }

        public static implicit operator string(AssemblyPath path)
        {
            return path._path;
        }

        public static implicit operator AssemblyPath(string path)
        {
            return new AssemblyPath(path);
        }
    }
}